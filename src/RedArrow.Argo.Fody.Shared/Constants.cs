namespace RedArrow.Argo
{
    public static class Constants
    {
        public static readonly string ArgoAssemblyName = "RedArrow.Argo";

        public static class Attributes
        {
            private static readonly string Package = $"{ArgoAssemblyName}.Attributes";

            public static readonly string Model = $"{Package}.ModelAttribute";
            public static readonly string Id = $"{Package}.IdAttribute";
            public static readonly string Property = $"{Package}.PropertyAttribute";
			public static readonly string HasOne = $"{Package}.HasOneAttribute";
			public static readonly string HasMany = $"{Package}.HasManyAttribute";

            public static readonly string Resource = $"{Package}.ResourceAttribute";
            public static readonly string Patch = $"{Package}.PatchAttribute";
        }
    }
}