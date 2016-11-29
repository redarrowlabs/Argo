namespace RedArrow.Jsorm
{
    public static class Constants
    {
        public static readonly string JsormAssemblyName = "RedArrow.Jsorm";

        public static class Attributes
        {
            private static readonly string Package = $"{JsormAssemblyName}.Attributes";

            public static readonly string Model = $"{Package}.ModelAttribute";
            public static readonly string Id = $"{Package}.IdAttribute";
            public static readonly string Property = $"{Package}.PropertyAttribute";
			public static readonly string HasOne = $"{Package}.HasOneAttribute";
			public static readonly string HasMany = $"{Package}.HasManyAttribute";
		}
    }
}