namespace API_First_Project.Dtos
{
    public class UserSettingDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public SettingDto Setting { get; set; } 

        public class SettingDto
        {
            public string Language { get; set; }
            public string Color { get; set; }
            public string Theme { get; set; }
        }
    }
}

