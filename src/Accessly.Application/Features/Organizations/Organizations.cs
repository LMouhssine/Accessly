using Accessly.Application.Common;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Common;
using Accessly.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Organizations;

public sealed record OrganizationDto(Guid Id, string Name, string Slug, DateTimeOffset CreatedAt);

public sealed record GetOrganizationsQuery : IQuery<IReadOnlyList<OrganizationDto>>;

public sealed class GetOrganizationsHandler(IAppDbContext db)
    : IRequestHandler<GetOrganizationsQuery, IReadOnlyList<OrganizationDto>>
{
    public async Task<IReadOnlyList<OrganizationDto>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
        => await db.Organizations.AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto(o.Id, o.Name, o.Slug, o.CreatedAt))
            .ToListAsync(cancellationToken);
}

public sealed record CreateOrganizationCommand(string Name) : ICommand<OrganizationDto>;

public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
}

public sealed class CreateOrganizationHandler(IAppDbContext db, IAuditLogger audit)
    : IRequestHandler<CreateOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var slug = await UniqueSlugAsync(db, SlugGenerator.Generate(request.Name), cancellationToken);
        var organization = new Organization { Name = request.Name.Trim(), Slug = slug };

        db.Organizations.Add(organization);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.OrganizationCreated, nameof(Organization), organization.Id.ToString(), new { organization.Name }, cancellationToken);

        return new OrganizationDto(organization.Id, organization.Name, organization.Slug, organization.CreatedAt);
    }

    private static async Task<string> UniqueSlugAsync(IAppDbContext db, string baseSlug, CancellationToken cancellationToken)
    {
        var slug = baseSlug;
        var suffix = 1;
        while (await db.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{++suffix}";
        }

        return slug;
    }
}
