using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Who receives the notification; drives the wording (plan decision D5).
/// Guardian = Patient account acting for a dependent.
/// </summary>
public enum NotificationRecipientRole
{
    Doctor,
    Patient,
    Guardian
}

/// <summary>
/// Everything the composer needs to render one notification. Values are collected
/// by the caller (NotificationQueries for rendering, PushDispatchService for push);
/// patient names shown to a doctor must already be masked by the shared INameMasker
/// upstream. Doctor names shown to patients are real.
/// </summary>
public sealed class NotificationRenderData
{
    public required NotificationType Type { get; init; }
    public required NotificationRecipientRole RecipientRole { get; init; }
    public required string DoctorName { get; init; }
    public required string DoctorSpecialization { get; init; }
    public required string MaskedPatientName { get; init; }

    /// <summary>Real name of the dependent; set when the recipient is a guardian.</summary>
    public string? DependentName { get; init; }

    public required string WorkplaceName { get; init; }

    /// <summary>Reserved for the future LabResult feature.</summary>
    public string? LabType { get; init; }
}

/// <summary>
/// T04: composes notification text at READ time (never stored, plan decision D4).
/// Arabic wording comes from spec section 4; push wording for AccessRequested /
/// EmergencyAccess from spec section 8. Compose lists one branch per
/// NotificationType with NO default arm: adding a new enum value must break the
/// build instead of silently rendering wrong text.
/// </summary>
public class NotificationComposer
{
    /// <summary>Title/body for the REST list (spec section 4 table).</summary>
    // CS8524 is expected here: it warns about UNNAMED enum values such as
    // (NotificationType)0. Every NAMED value is covered; the plan forbids a
    // default arm so that adding a new named value breaks the build (CS8509).
#pragma warning disable CS8524
    public (string Title, string Body) Compose(NotificationRenderData d) =>
        d.Type switch
        {
            NotificationType.AccessRequested => ComposeAccessRequested(d),
            NotificationType.EmergencyAccess => ("دخول طوارئ لسجلك الطبي",
                $"وصل د. {d.DoctorName} لسجلك عبر الطوارئ."),
            NotificationType.RequestAccepted => ("تم قبول طلب الوصول",
                $"المريض {d.MaskedPatientName} وافق على طلب وصولك."),
            NotificationType.AccessExpired => d.RecipientRole == NotificationRecipientRole.Doctor
                ? ("انتهت صلاحية الوصول", $"انتهت فترة الوصول بينك وبين المريض {d.MaskedPatientName}.")
                : ("انتهت صلاحية الوصول", $"تم إغلاق وصول د. {d.DoctorName} تلقائيًا لانتهاء المدة."),
            NotificationType.AccessClosed => d.RecipientRole == NotificationRecipientRole.Doctor
                ? ("انتهى الوصول", $"تم إنهاء الوصول مع المريض {d.MaskedPatientName}.")
                : ("انتهى الوصول", $"أنهى د. {d.DoctorName} الوصول إلى سجلك."),
            NotificationType.AccessExtended => ("تم تمديد الوصول",
                $"لقد قام المريض {d.MaskedPatientName} بتمديد الوصول بينكما."),
            NotificationType.RequestExpired => d.RecipientRole == NotificationRecipientRole.Doctor
                ? ("لم يُقبل طلب الوصول", $"لم يُقبل طلب وصولك للمريض {d.MaskedPatientName}.")
                : ("لم يُقبل طلب الوصول", $"انتهت مدة الرد على طلب د. {d.DoctorName} دون قرار منك."),
            NotificationType.LabResult => ("نتيجة فحص جديدة",
                $"تم إرسال {d.LabType} إلى ملفك الطبي."),
            NotificationType.GuardianLinked => ("تم ربط حساب جديد",
                $"تمت إضافة الابن {d.DependentName} بنجاح تحت وصايتك."),
            NotificationType.MedicalVisit => ("زيارة طبية جديدة",
                $"أضاف د. {d.DoctorName} تقرير الكشف لسجلك."),
        };
#pragma warning restore CS8524

    /// <summary>
    /// Title/body for the FCM push (spec section 8). Only AccessRequested and
    /// EmergencyAccess have their own wording; every other type uses the same
    /// text as the table. Guardian: the dependent's name is added to the title.
    /// </summary>
    public (string Title, string Body) ComposePush(NotificationRenderData d) =>
        d.Type switch
        {
            NotificationType.AccessRequested => (
                d.RecipientRole == NotificationRecipientRole.Guardian
                    ? $"دكتور {d.DoctorName} أرسل إليك طلب وصول لسجل الابن {d.DependentName}."
                    : $"دكتور {d.DoctorName} أرسل إليك طلب وصول.",
                d.DoctorSpecialization),
            NotificationType.EmergencyAccess => (
                d.RecipientRole == NotificationRecipientRole.Guardian
                    ? $"دخل الطبيب {d.DoctorName} على سجل الابن {d.DependentName} في حالة طوارئ."
                    : $"دخل الطبيب {d.DoctorName} على سجلك في حالة طوارئ.",
                $"تم تسجيل الدخول من {d.WorkplaceName}"),
            _ => Compose(d)
        };

    private static (string Title, string Body) ComposeAccessRequested(NotificationRenderData d) =>
        d.RecipientRole == NotificationRecipientRole.Guardian
            ? ($"طلب وصول لسجل الابن {d.DependentName}", $"يرغب د. {d.DoctorName} في الاطلاع على سجلك.")
            : ("طلب وصول جديد", $"يرغب د. {d.DoctorName} في الاطلاع على سجلك.");
}
