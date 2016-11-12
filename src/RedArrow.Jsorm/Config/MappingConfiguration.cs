namespace RedArrow.Jsorm.Config
{
    /// <summary>
    /// This doesn't appear to do much right now, but in the near future, this
    /// would be responsible for merging the automapped configuration with the
    /// dev overrides, and then applying that merged configuration to the
    /// SessionConfiguration
    /// </summary>
    public class MappingConfiguration
    {
        // TODO
        // private AutoMapsStore AutoMaps { get; }
        public ResourceMapsStore ResourceMaps { get; }

        public MappingConfiguration()
        {
            // AutoMaps = new AutoMapsStore();
            ResourceMaps = new ResourceMapsStore();
        }

        public void Configure(SessionConfiguration config)
        {
            //AutoMaps.Configure(config);
            ResourceMaps.Configure(config);
        }
    }
}