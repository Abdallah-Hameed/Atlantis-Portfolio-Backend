using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class PortfolioProfile
{
    public int PortfolioProfileId { get; set; }

    public string FullName { get; set; } = null!;

    public string ProfessionalTitle { get; set; } = null!;

    public string Bio { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public string? Email { get; set; }

    public string? Location { get; set; }
}
