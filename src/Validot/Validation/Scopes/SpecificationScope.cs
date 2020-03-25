namespace Validot.Validation.Scopes
{
    using System.Collections.Generic;

    internal class SpecificationScope<T> : ISpecificationScope<T>
    {
        private static readonly bool IsClass = typeof(T).IsClass && default(T) == null;

        public IReadOnlyList<ICommandScope<T>> CommandScopes { get; set; }

        public Presence Presence { get; set; }

        public int ForbiddenErrorId { get; set; }

        public int RequiredErrorId { get; set; }

        public void Discover(IDiscoveryContext context)
        {
            if (IsClass)
            {
                if (Presence == Presence.Forbidden)
                {
                    context.AddError(ForbiddenErrorId);

                    return;
                }

                if (Presence == Presence.Required)
                {
                    context.AddError(RequiredErrorId);
                }
            }

            for (var i = 0; i < CommandScopes.Count; ++i)
            {
                CommandScopes[i].Discover(context);
            }
        }

        public void Validate(T model, IValidationContext context)
        {
            if (IsClass)
            {
                if (model == null && Presence == Presence.Required)
                {
                    context.AddError(RequiredErrorId);

                    return;
                }

                if (model != null && Presence == Presence.Forbidden)
                {
                    context.AddError(ForbiddenErrorId);

                    return;
                }
            }

            for (var i = 0; i < CommandScopes.Count; ++i)
            {
                CommandScopes[i].Validate(model, context);

                if (context.ShouldFallBack)
                {
                    return;
                }
            }
        }
    }
}