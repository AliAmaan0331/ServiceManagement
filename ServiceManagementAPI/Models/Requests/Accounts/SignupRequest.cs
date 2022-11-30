namespace ServiceManagementAPI.Models.Requests.Accounts
{
    public class SignupRequest
    {
        public virtual string? Name { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? Password { get; set; }
        public virtual string? BusinessName { get; set; }
        public virtual long CountryId { get; set; }
        public virtual long StateId { get; set; }
        public virtual long CityId { get; set; }
        public virtual string? TimeZone { get; set; }
        public virtual string? PhoneNumber { get; set; }
    }
}
