namespace PlaygroundMediator.Constants
{
    public static class PolicyNames
    {
        // Políticas basadas en permisos
        public const string CanReadProducts = "CanReadProducts";
        public const string CanCreateProducts = "CanCreateProducts";
        public const string CanUpdateProducts = "CanUpdateProducts";
        public const string CanDeleteProducts = "CanDeleteProducts";

        // Políticas basadas en roles
        public const string IsAdmin = "IsAdmin";

        // Combinar roles y permisos si es necesario
        public const string AdminCanCreateProducts = "AdminCanCreateProducts";
        public const string AdminOrUserCanCreateProducts = "AdminOrUserCanCreateProducts";
    }
}