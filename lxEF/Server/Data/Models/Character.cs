using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace lxEF.Server.Data.Models
{
    public class Character
    {
        [Key]
        public string CitizenID { get; private set; }

        public int CharacterID { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public int Age { get; private set; }

        public string Nationality { get; private set; }

        public DateTime DateOfBirth { get; private set; }

        public string Gender { get; private set; }

        //public string PhoneNumber { get; private set; }

        //public string Height { get; private set; }

        //public string Weight { get; private set; }

        //public string Fingerprint { get; private set; }

        //public string BloodType { get; private set; }

        public bool IsDrunk { get; private set; }

        public bool IsHigh { get; private set; }

        //public List<string> CurrentDrugs { get; private set; }

        public string Ped { get; private set; }

        [JsonIgnore]
        public DBUser User { get; private set; }



        // and more and more and more....

        public Character()
        {

        }

        public Character(string firstName, string lastName, int age, DateTime dateOfBirth, string gender, string nationality, DBUser user, string ped = "")
        {
            CitizenID = GenerateCitizenID(dateOfBirth);
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Nationality = nationality;
            //PhoneNumber = phoneNumber;
            //Height = height;
            //Weight = weight;
            //Fingerprint = Guid.NewGuid().ToString();
            //BloodType = bloodType;
            IsDrunk = false;
            IsHigh = false;
            //CurrentDrugs = new List<string>();
            User = user;
            Ped = ped;
            CharacterID = user.Characters.Count + 1;
        }

        private string GenerateCitizenID(DateTime dateOfBirth)
        {
            Random random = new Random();
            string randomDigits = string.Join("", Enumerable.Range(0, 4).Select(_ => random.Next(10)));
            return $"{dateOfBirth.Year % 100:00}{dateOfBirth.Month:00}{dateOfBirth.Day:00}{randomDigits}";
        }
    }
}
