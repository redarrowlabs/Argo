using System;

namespace WovenByFody
{
    public class SystemMetadata
    {
        public DateTime CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Guid UpdatedBy { get; set; }

        public Guid ETag { get; set; }
    }
}