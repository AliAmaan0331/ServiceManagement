using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagementDAL
{
    public class BaseRepository
    {
        private readonly string? _connectionString;
        public BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private static string? _encapsulation;
        private static readonly ConcurrentDictionary<Type, string> TableNames = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<string, string> ColumnNames = new ConcurrentDictionary<string, string>();
        private static ITableNameResolver _tableNameResolver = new TableNameResolver();
        private static IColumnNameResolver _columnNameResolver = new ColumnNameResolver();
        private static readonly ConcurrentDictionary<string, string> StringBuilderCacheDict = new ConcurrentDictionary<string, string>();
        private static bool StringBuilderCacheEnabled = true;
        internal dynamic Connection => new NpgsqlConnection(_connectionString);

        public async Task<T> GetAllAsync<T>(string whereConditions, IDbTransaction transaction = null, int? commandTimeout = null, bool includeActive=true)
        {
            var currenttype = typeof(T);
            var name = GetTableName(currenttype);

            var sb = new StringBuilder();
            var whereprops = GetAllProperties(whereConditions).ToArray();
            sb.Append("Select ");
            //create a new empty instance of the type to get the base properties
            BuildSelect(sb, GetScaffoldableProperties<T>().ToArray());
            sb.AppendFormat(" from {0}", name);


            if (whereprops.Any())
            {
                sb.Append(" where ");
                BuildWhere<T>(sb, whereprops, whereConditions);
            }


            if (includeActive && whereprops.Any())
            {
                sb.Append(" AND Active = '1'");
            }
            else if (includeActive)
                sb.Append(" Where Active = '1'");

            using (var connection = Connection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                var result = connection.Query<T>(sb.ToString().Replace(@"""", ""), whereConditions, transaction, true, commandTimeout);
                connection.Close();
                return await result;
            }
        }

        public async Task DeleteRowAsync(long id)
        {
            using (var connection = Connection)
            {
                await connection.ExecuteAsync($"DELETE FROM {_tableName} WHERE Id=@Id", new { Id = id });
            }
        }

        public async Task GetAsync(long id)
        {
            using (var connection = Connection)
            {
                var result = await connection.QuerySingleOrDefaultAsync($"SELECT * FROM {_tableName} WHERE Id=@Id", new { Id = id });
                if (result == null)
                    throw new KeyNotFoundException($"{_tableName} with id [{id}] could not be found.");

                return result;
            }
        }

        public async Task<int> SaveRangeAsync(IEnumerable list)
        {
            var inserted = 0;
            var query = GenerateInsertQuery();
            using (var connection = Connection)
            {
                inserted += await connection.ExecuteAsync(query, list);
            }

            return inserted;
        }
        private static void StringBuilderCache(StringBuilder sb, string cacheKey, Action<StringBuilder> stringBuilderAction)
        {
            if (StringBuilderCacheEnabled && StringBuilderCacheDict.TryGetValue(cacheKey, out string value))
            {
                sb.Append(value);
                return;
            }

            StringBuilder newSb = new StringBuilder();
            stringBuilderAction(newSb);
            value = newSb.ToString();
            StringBuilderCacheDict.AddOrUpdate(cacheKey, value, (t, v) => value);
            sb.Append(value);
        }
        public static void SetTableNameResolver(ITableNameResolver resolver)
        {
            _tableNameResolver = resolver;
        }

        /// <summary>
        /// Sets the column name resolver
        /// </summary>
        /// <param name="resolver">The resolver to use when requesting the format of a column name</param>
        public static void SetColumnNameResolver(IColumnNameResolver resolver)
        {
            _columnNameResolver = resolver;
        }

        //build update statement based on list on an entity
        private static void BuildUpdateSet<T>(T entityToUpdate, StringBuilder masterSb)
        {
            StringBuilderCache(masterSb, $"{typeof(T).FullName}_BuildUpdateSet", sb =>
            {
                var nonIdProps = GetUpdateableProperties(entityToUpdate).ToArray();

                for (var i = 0; i < nonIdProps.Length; i++)
                {
                    var property = nonIdProps[i];

                    sb.AppendFormat("{0} = @{1}", GetColumnName(property), property.Name);
                    if (i < nonIdProps.Length - 1)
                        sb.AppendFormat(", ");
                }
            });
        }

        //build select clause based on list of properties skipping ones with the IgnoreSelect and NotMapped attribute
        private static void BuildSelect(StringBuilder masterSb, IEnumerable<PropertyInfo> props)
        {
            StringBuilderCache(masterSb, $"{props.CacheKey()}_BuildSelect", sb =>
            {
                var propertyInfos = props as IList<PropertyInfo> ?? props.ToList();
                var addedAny = false;
                for (var i = 0; i < propertyInfos.Count(); i++)
                {
                    var property = propertyInfos.ElementAt(i);

                    if (property.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(IgnoreSelectAttribute).Name || attr.GetType().Name == typeof(NotMappedAttribute).Name)) continue;

                    if (addedAny)
                        sb.Append(",");
                    sb.Append(GetColumnName(property));
                    //if there is a custom column name add an "as customcolumnname" to the item so it maps properly
                    if (property.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) != null)
                        sb.Append(" as " + Encapsulate(property.Name));
                    addedAny = true;
                }
            });
        }

        private static void BuildWhere<TEntity>(StringBuilder sb, IEnumerable<PropertyInfo> idProps, object whereConditions = null)
        {
            var propertyInfos = idProps.ToArray();
            for (var i = 0; i < propertyInfos.Count(); i++)
            {
                var useIsNull = false;

                //match up generic properties to source entity properties to allow fetching of the column attribute
                //the anonymous object used for search doesn't have the custom attributes attached to them so this allows us to build the correct where clause
                //by converting the model type to the database column name via the column attribute
                var propertyToUse = propertyInfos.ElementAt(i);
                var sourceProperties = GetScaffoldableProperties<TEntity>().ToArray();
                for (var x = 0; x < sourceProperties.Count(); x++)
                {
                    if (sourceProperties.ElementAt(x).Name == propertyToUse.Name)
                    {
                        if (whereConditions != null && propertyToUse.CanRead && (propertyToUse.GetValue(whereConditions, null) == null || propertyToUse.GetValue(whereConditions, null) == DBNull.Value))
                        {
                            useIsNull = true;
                        }
                        propertyToUse = sourceProperties.ElementAt(x);
                        break;
                    }
                }
                sb.AppendFormat(
                    useIsNull ? "{0} is null" : "{0} = @{1}",
                    GetColumnName(propertyToUse),
                    propertyToUse.Name);

                if (i < propertyInfos.Count() - 1)
                    sb.AppendFormat(" and ");
            }



        }

        private static string BuildWhere<TEntity>(Dictionary<string, object> conditions, string[] parameters, string comparisonOperator = "=", bool includeQuotation = true)
        {
            string where = " WHERE 1=1  ";
            Dictionary<string, Type> isColumnNumericDic = new Dictionary<string, Type>();

            var sourceProperties = GetScaffoldableProperties<TEntity>().ToArray();
            for (var x = 0; x < sourceProperties.Count(); x++)
            {
                var val = sourceProperties[x].GetCustomAttributes();
                var abc = val.GetType();
                var columnAttribute = val.Where(x => x.GetType() == typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)).FirstOrDefault();
                if (columnAttribute != null)
                {
                    Type t = Nullable.GetUnderlyingType(sourceProperties[x].PropertyType);
                    //isColumnNumericDic.Add((columnAttribute as System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).Name, sourceProperties[x].PropertyType == typeof(Nullable) ? Nullable.GetUnderlyingType(sourceProperties[x].PropertyType) : sourceProperties[x].PropertyType);

                    var type = t == null ? sourceProperties[x].PropertyType : t;
                    isColumnNumericDic.Add((columnAttribute as System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).Name, type);

                    conditions.ToList<KeyValuePair<string, object>>().ForEach(q =>
                    {
                        if (sourceProperties.ElementAt(x).CustomAttributes.First(a => a.AttributeType.Name == "ColumnAttribute").ConstructorArguments[0].Value.ToString().ToLower() == q.Key.Replace("@", "").ToLower())
                        {
                            if (type.Name.ToLower().Contains("int") || type.Name.ToLower().Contains("short"))
                                where += string.Format(" and {0} {2} ({1})", q.Key.Replace("@", ""), q.Value, "in");
                            else if (type.Name.ToLower().Contains("date"))
                                where += string.Format(" and {0} {2} {1}", q.Key.Replace("@", ""), q.Value.ToString().Replace("@", ""), " ");
                            else
                            {
                                if (includeQuotation)
                                    where += string.Format(" and {0} {2} '{1}'", q.Key.Replace("@", ""), q.Value, comparisonOperator);
                                else
                                    where += string.Format(" and {0} {2} {1}", q.Key.Replace("@", ""), q.Value, comparisonOperator);
                            }
                        }
                    });
                }
            }
            if (conditions.Keys.FirstOrDefault(x => x == "@keyfilter") != null)
            {
                string keyFilterValue = conditions.FirstOrDefault(x => x.Key == "@keyfilter").Value.ToString();
                where += " AND ( 1!=1 filterWhere )";
                for (int i = 0; i < parameters.Length; i++)
                {
                    var dicItemType = isColumnNumericDic.Where(x => x.Key.ToLower().Equals(parameters[i].ToLower())).Select(x => x.Value).FirstOrDefault();
                    if (dicItemType != null && IsNumericType(dicItemType))
                    {
                        if (double.TryParse(keyFilterValue, out double doubleResult) || int.TryParse(keyFilterValue, out int intResult))
                            where = where.Replace("filterWhere", string.Format("OR {1} ilike '%' + '{0}' + '%'filterWhere ", keyFilterValue, parameters[i]));
                    }
                    else if (dicItemType != null && IsDateType(dicItemType))
                    {
                        if (DateTime.TryParse(keyFilterValue, out DateTime datetimeResult) || double.TryParse(keyFilterValue, out double doubleResult) || int.TryParse(keyFilterValue, out int intResult))
                            where = where.Replace("filterWhere", string.Format("OR {1} ilike '%' + '{0}' + '%'filterWhere ", keyFilterValue, parameters[i]));
                    }
                    else
                        where = where.Replace("filterWhere", string.Format("OR {1} ilike '%{0}%' filterWhere ", keyFilterValue, parameters[i]));
                }
                where = where.Replace("filterWhere", "");
            }

            return where;
        }
        //build insert values which include all properties in the class that are:
        //Not named Id
        //Not marked with the Editable(false) attribute
        //Not marked with the [Key] attribute (without required attribute)
        //Not marked with [IgnoreInsert]
        //Not marked with [NotMapped]
        private static void BuildInsertValues<T>(StringBuilder masterSb)
        {
            StringBuilderCache(masterSb, $"{typeof(T).FullName}_BuildInsertValues", sb =>
            {

                var props = GetScaffoldableProperties<T>().ToArray();
                for (var i = 0; i < props.Count(); i++)
                {
                    var property = props.ElementAt(i);
                    if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(string)
                          && property.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)
                          && property.GetCustomAttributes(true).All(attr => attr.GetType().Name != typeof(RequiredAttribute).Name))
                        continue;
                    if (property.GetCustomAttributes(true).Any(attr =>
                        attr.GetType().Name == typeof(IgnoreInsertAttribute).Name ||
                        attr.GetType().Name == typeof(NotMappedAttribute).Name ||
                        attr.GetType().Name == typeof(ReadOnlyAttribute).Name && IsReadOnly(property))
                    ) continue;

                    if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && property.GetCustomAttributes(true).All(attr => attr.GetType().Name != typeof(RequiredAttribute).Name) && property.PropertyType != typeof(Guid)) continue;

                    sb.AppendFormat("@{0}", property.Name);
                    if (i < props.Count() - 1)
                        sb.Append(", ");
                }
                if (sb.ToString().Replace(@"""", "").EndsWith(", "))
                    sb.Remove(sb.Length - 2, 2);
            });
        }

        //build insert parameters which include all properties in the class that are not:
        //marked with the Editable(false) attribute
        //marked with the [Key] attribute
        //marked with [IgnoreInsert]
        //named Id
        //marked with [NotMapped]
        private static void BuildInsertParameters<T>(StringBuilder masterSb)
        {
            StringBuilderCache(masterSb, $"{typeof(T).FullName}_BuildInsertParameters", sb =>
            {
                var props = GetScaffoldableProperties<T>().ToArray();

                for (var i = 0; i < props.Count(); i++)
                {
                    var property = props.ElementAt(i);

                    if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(string)
                          && property.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)
                          && property.GetCustomAttributes(true).All(attr => attr.GetType().Name != typeof(RequiredAttribute).Name))
                        continue;



                    if (property.GetCustomAttributes(true).Any(attr =>
                        attr.GetType().Name == typeof(IgnoreInsertAttribute).Name ||
                        attr.GetType().Name == typeof(NotMappedAttribute).Name ||
                        attr.GetType().Name == typeof(ReadOnlyAttribute).Name && IsReadOnly(property))) continue;

                    if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                    property.GetCustomAttributes(true).All(attr => attr.GetType().Name != typeof(RequiredAttribute).Name)
                    && property.PropertyType != typeof(Guid))
                        continue;


                    sb.Append(GetColumnName(property));
                    if (i < props.Count() - 1)
                        sb.Append(", ");
                }
                if (sb.ToString().Replace(@"""", "").EndsWith(", "))
                    sb.Remove(sb.Length - 2, 2);
            });
        }

        //Get all properties in an entity
        private static IEnumerable<PropertyInfo> GetAllProperties<T>(T entity) where T : class
        {
            if (entity == null) return new PropertyInfo[0];
            return entity.GetType().GetProperties();
        }

        //Get all properties that are not decorated with the Editable(false) attribute
        private static IEnumerable<PropertyInfo> GetScaffoldableProperties<T>()
        {
            IEnumerable<PropertyInfo> props = typeof(T).GetProperties();

            props = props.Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(EditableAttribute).Name && !IsEditable(p)) == false);


            return props.Where(p => p.PropertyType.IsSimpleType() || IsEditable(p));
        }

        //Determine if the Attribute has an AllowEdit key and return its boolean state
        //fake the funk and try to mimic EditableAttribute in System.ComponentModel.DataAnnotations 
        //This allows use of the DataAnnotations property in the model and have the SimpleCRUD engine just figure it out without a reference
        private static bool IsEditable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0)
            {
                dynamic write = attributes.FirstOrDefault(x => x.GetType().Name == typeof(EditableAttribute).Name);
                if (write != null)
                {
                    return write.AllowEdit;
                }
            }
            return false;
        }


        //Determine if the Attribute has an IsReadOnly key and return its boolean state
        //fake the funk and try to mimic ReadOnlyAttribute in System.ComponentModel 
        //This allows use of the DataAnnotations property in the model and have the SimpleCRUD engine just figure it out without a reference
        private static bool IsReadOnly(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0)
            {
                dynamic write = attributes.FirstOrDefault(x => x.GetType().Name == typeof(ReadOnlyAttribute).Name);
                if (write != null)
                {
                    return write.IsReadOnly;
                }
            }
            return false;
        }

        //Get all properties that are:
        //Not named Id
        //Not marked with the Key attribute
        //Not marked ReadOnly
        //Not marked IgnoreInsert
        //Not marked NotMapped
        private static IEnumerable<PropertyInfo> GetUpdateableProperties<T>(T entity)
        {
            var updateableProperties = GetScaffoldableProperties<T>();
            //remove ones with ID
            updateableProperties = updateableProperties.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            //remove ones with key attribute
            updateableProperties = updateableProperties.Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name) == false);
            //remove ones that are readonly
            updateableProperties = updateableProperties.Where(p => p.GetCustomAttributes(true).Any(attr => (attr.GetType().Name == typeof(ReadOnlyAttribute).Name) && IsReadOnly(p)) == false);
            //remove ones with IgnoreUpdate attribute
            updateableProperties = updateableProperties.Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(IgnoreUpdateAttribute).Name) == false);
            //remove ones that are not mapped
            updateableProperties = updateableProperties.Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(NotMappedAttribute).Name) == false);

            return updateableProperties;
        }

        //Get all properties that are named Id or have the Key attribute
        //For Inserts and updates we have a whole entity so this method is used
        private static IEnumerable<PropertyInfo> GetIdProperties(object entity)
        {
            var type = entity.GetType();
            return GetIdProperties(type);
        }

        //Get all properties that are named Id or have the Key attribute
        //For Get(id) and Delete(id) we don't have an entity, just the type so this method is used
        private static IEnumerable<PropertyInfo> GetIdProperties(Type type)
        {
            var tp = type.GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).ToList();
            return tp.Any() ? tp : type.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        }

        //Gets the table name for this entity
        //For Inserts and updates we have a whole entity so this method is used
        //Uses class name by default and overrides if the class has a Table attribute
        private static string GetTableName(object entity)
        {
            var type = entity.GetType();
            return GetTableName(type);
        }

        //Gets the table name for this type
        //For Get(id) and Delete(id) we don't have an entity, just the type so this method is used
        //Use dynamic type to be able to handle both our Table-attribute and the DataAnnotation
        //Uses class name by default and overrides if the class has a Table attribute
        private static string GetTableName(Type type)
        {
            string tableName;

            if (TableNames.TryGetValue(type, out tableName))
                return tableName;

            tableName = _tableNameResolver.ResolveTableName(type);

            TableNames.AddOrUpdate(type, tableName, (t, v) => tableName);

            return tableName;
        }

        private static string GetColumnName(PropertyInfo propertyInfo)
        {
            string columnName, key = string.Format("{0}.{1}", propertyInfo.DeclaringType, propertyInfo.Name);

            if (ColumnNames.TryGetValue(key, out columnName))
                return columnName;

            columnName = _columnNameResolver.ResolveColumnName(propertyInfo);

            ColumnNames.AddOrUpdate(key, columnName, (t, v) => columnName);

            return columnName;
        }

        private static string Encapsulate(string databaseword)
        {
            return string.Format(_encapsulation, databaseword);
        }
        public static bool IsNumericType(Type t)
        {
            Type underLyingType = Nullable.GetUnderlyingType(t);
            TypeCode typeCode = TypeCode.String;
            if (underLyingType == null)
                typeCode = Type.GetTypeCode(t);
            else
                typeCode = Type.GetTypeCode(underLyingType);
            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDateType(Type t)
        {
            Type underLyingType = Nullable.GetUnderlyingType(t);
            TypeCode typeCode = TypeCode.String;
            if (underLyingType == null)
                typeCode = Type.GetTypeCode(t);
            else
                typeCode = Type.GetTypeCode(underLyingType);
            switch (typeCode)
            {
                case TypeCode.DateTime:
                    return true;
                default:
                    return false;
            }
        }
        public interface ITableNameResolver
        {
            string ResolveTableName(Type type);
        }

        public interface IColumnNameResolver
        {
            string ResolveColumnName(PropertyInfo propertyInfo);
        }

        public class TableNameResolver : ITableNameResolver
        {
            public virtual string ResolveTableName(Type type)
            {
                var tableName = Encapsulate(type.Name);

                var tableattr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
                if (tableattr != null)
                {
                    tableName = Encapsulate(tableattr.Name);
                    try
                    {
                        if (!String.IsNullOrEmpty(tableattr.Schema))
                        {
                            string schemaName = Encapsulate(tableattr.Schema);
                            tableName = String.Format("{0}.{1}", schemaName, tableName);
                        }
                    }
                    catch (RuntimeBinderException)
                    {
                        //Schema doesn't exist on this attribute.
                    }
                }

                return tableName;
            }
        }

        public class ColumnNameResolver : IColumnNameResolver
        {
            public virtual string ResolveColumnName(PropertyInfo propertyInfo)
            {
                var columnName = Encapsulate(propertyInfo.Name);

                var columnattr = propertyInfo.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) as dynamic;
                if (columnattr != null)
                {
                    columnName = Encapsulate(columnattr.Name);
                }
                return columnName;
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        public class TableAttribute : Attribute
        {
            /// <summary>
            /// Optional Table attribute.
            /// </summary>
            /// <param name="tableName"></param>
            public TableAttribute(string tableName)
            {
                Name = tableName;
            }
            /// <summary>
            /// Name of the table
            /// </summary>
            public string Name { get; private set; }
            /// <summary>
            /// Name of the schema
            /// </summary>
            public string Schema { get; set; }
        }


        //[AttributeUsage(AttributeTargets.Class)]
        //public class RepoAttribute : Attribute
        //{
        //    /// <summary>
        //    /// Optional Table attribute.
        //    /// </summary>
        //    /// <param name="tableName"></param>
        //    public RepoAttribute(string repoName)
        //    {
        //        Name = repoName;
        //    }
        //    /// <summary>
        //    /// Name of the table
        //    /// </summary>
        //    public string Name { get; private set; }

        //}

        /// <summary>
        /// Optional Column attribute.
        /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class ColumnAttribute : Attribute
        {
            /// <summary>
            /// Optional Column attribute.
            /// </summary>
            /// <param name="columnName"></param>
            public ColumnAttribute(string columnName)
            {
                Name = columnName;
            }
            /// <summary>
            /// Name of the column
            /// </summary>
            public string Name { get; private set; }
        }

        /// <summary>
        /// Optional Key attribute.
        /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the Primary Key of a poco
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class KeyAttribute : Attribute
        {
        }

        /// <summary>
        /// Optional NotMapped attribute.
        /// You can use the System.ComponentModel.DataAnnotations version in its place to specify that the property is not mapped
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class NotMappedAttribute : Attribute
        {
        }

        /// <summary>
        /// Optional Key attribute.
        /// You can use the System.ComponentModel.DataAnnotations version in its place to specify a required property of a poco
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class RequiredAttribute : Attribute
        {
        }

        /// <summary>
        /// Optional Editable attribute.
        /// You can use the System.ComponentModel.DataAnnotations version in its place to specify the properties that are editable
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class EditableAttribute : Attribute
        {
            /// <summary>
            /// Optional Editable attribute.
            /// </summary>
            /// <param name="iseditable"></param>
            public EditableAttribute(bool iseditable)
            {
                AllowEdit = iseditable;
            }
            /// <summary>
            /// Does this property persist to the database?
            /// </summary>
            public bool AllowEdit { get; private set; }
        }

        /// <summary>
        /// Optional Readonly attribute.
        /// You can use the System.ComponentModel version in its place to specify the properties that are editable
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class ReadOnlyAttribute : Attribute
        {
            /// <summary>
            /// Optional ReadOnly attribute.
            /// </summary>
            /// <param name="isReadOnly"></param>
            public ReadOnlyAttribute(bool isReadOnly)
            {
                IsReadOnly = isReadOnly;
            }
            /// <summary>
            /// Does this property persist to the database?
            /// </summary>
            public bool IsReadOnly { get; private set; }
        }

        /// <summary>
        /// Optional IgnoreSelect attribute.
        /// Custom for Dapper.SimpleCRUD to exclude a property from Select methods
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class IgnoreSelectAttribute : Attribute
        {
        }

        /// <summary>
        /// Optional IgnoreInsert attribute.
        /// Custom for Dapper.SimpleCRUD to exclude a property from Insert methods
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class IgnoreInsertAttribute : Attribute
        {
        }

        /// <summary>
        /// Optional IgnoreUpdate attribute.
        /// Custom for Dapper.SimpleCRUD to exclude a property from Update methods
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class IgnoreUpdateAttribute : Attribute
        {
        }
    }
    internal static class TypeExtension
    {
        //You can't insert or update complex types. Lets filter them out.
        public static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            var simpleTypes = new List<Type>
                               {
                                   typeof(byte),
                                   typeof(sbyte),
                                   typeof(short),
                                   typeof(ushort),
                                   typeof(int),
                                   typeof(uint),
                                   typeof(long),
                                   typeof(ulong),
                                   typeof(float),
                                   typeof(double),
                                   typeof(decimal),
                                   typeof(bool),
                                   typeof(string),
                                   typeof(char),
                                   typeof(Guid),
                                   typeof(DateTime),
                                   typeof(DateTimeOffset),
                                   typeof(TimeSpan),
                                   typeof(byte[])
                               };
            return simpleTypes.Contains(type) || type.IsEnum;
        }

        public static string CacheKey(this IEnumerable<PropertyInfo> props)
        {
            return string.Join(",", props.Select(p => p.DeclaringType.FullName + "." + p.Name).ToArray());
        }
    }
}
