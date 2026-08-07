using NimbusBoard.Application.Projects;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Application.Common.Utils;

public static class MemberAvatarHelper
{
    private static readonly string[] Classes =
    [
        "bg-violet-100 text-violet-700",
        "bg-sky-100 text-sky-700",
        "bg-emerald-100 text-emerald-700",
        "bg-amber-100 text-amber-700",
        "bg-rose-100 text-rose-700",
        "bg-indigo-100 text-indigo-700"
    ];

    public static string ClassFor(int memberId) =>
        Classes[Math.Abs(memberId) % Classes.Length];

    public static ProjectMemberViewModel ToViewModel(ProjectMember member) => new()
    {
        MemberId = member.MemberId,
        DisplayName = member.DisplayName,
        Initials = member.Initials,
        Role = member.Role.ToString(),
        AvatarClass = ClassFor(member.MemberId)
    };
}
