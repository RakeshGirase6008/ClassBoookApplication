using System;

namespace ClassBookApplication.Domain.Student
{
    public class Student : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public string ContactNo { get; set; }
        public string Pincode { get; set; }
        public int BoardId { get; set; }
        public int MediumId { get; set; }
        public int StandardId { get; set; }
        public string RegistrationNo { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string ReferCode { get; set; }
        public string UniqueNo { get; set; }
        public string QRcodeImageURL { get; set; }
        public int RegistrationFromTypeId { get; set; }
        public int RegistrationByTypeId { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
