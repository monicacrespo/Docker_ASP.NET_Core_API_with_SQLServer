namespace DockerGigApi.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Gig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GigId { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Gig Date")]
        public DateTime GigDate { get; set; } = DateTime.MinValue;

        public string MusicGenre { get; set; }
    }
}