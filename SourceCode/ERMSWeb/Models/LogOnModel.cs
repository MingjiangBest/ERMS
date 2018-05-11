using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERMSWeb.Models
{
    public class LogOnModel
    {
        public CheckState CheckState { get; set; }
        public string Message { get; set; }
        public UserDto User { get; set; }
        public string ApiHost { get; set; }
    }

    // check result 
    public enum CheckState
    {
        Success = 0,
        EmptyUserName = 1,
        EmptyPassword = 2,
        UserNameNotExist = 3,
        WrongPassword = 4,
        Inactive = 5,
        Expired = 6,
        UnKnown = 7,
        Unauthorized = 8
    }

    public class UserDto
    {
        public string UniqueID { get; set; }
        public string LoginName { get; set; }
        public string LocalName { get; set; }
        public string EnglishName { get; set; }
        public string Password { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string Comments { get; set; }
        public int? DeleteMark { get; set; }
        public byte[] SignImage { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string IkeySn { get; set; }
        public string Domain { get; set; }
        public string DisplayName { get; set; }
        public int? InvalidLoginCount { get; set; }
        public bool? IsLocked { get; set; }

        public bool HasValidPeriod { get; set; }
        public DateTime? ValidStartDate { get; set; }
        public DateTime? ValidEndDate { get; set; }
    }
}