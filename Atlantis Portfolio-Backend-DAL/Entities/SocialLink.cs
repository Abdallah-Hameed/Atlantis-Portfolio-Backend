using System;
using System.Collections.Generic;

namespace Atlantis_Portfolio_Backend_DAL.Entities;

public partial class SocialLink
{
    public int SocialLinkId { get; set; }

    public string Platform { get; set; } = null!;

    public string Url { get; set; } = null!;

    public short DisplayOrder { get; set; }
}
