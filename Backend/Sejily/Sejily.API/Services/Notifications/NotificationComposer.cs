using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Who receives the notification. The wording changes with the role.
/// Guardian = a parent/guardian who is notified about their dependent (child).
/// </summary>
public enum NotificationRecipientRole
{
    Doctor,
    Patient,
    Guardian
}

/// <summary>
/// Everything needed to write one notification's text. The caller fills it from the database.
/// A patient's name shown to a doctor must already be masked; a doctor's name shown to a patient is real.
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
/// Turns (notification type + who receives it + names) into the Arabic title and body.
/// The text is built every time it is shown and is NOT stored in the database.
/// Example: type AccessRequested for a patient gives the title "طلب وصول جديد" and the body
/// "يرغب د. أحمد في الاطلاع على سجلك.".
/// There is one case per notification type and no "default" case on purpose: if someone adds a
/// new type and forgets its text, the project fails to build instead of showing wrong text.
/// </summary>
public class NotificationComposer
{
    /// <summary>Title and body used when notifications are listed through the REST API.</summary>
    // The compiler warns (CS8524) about "unnamed" enum values such as (NotificationType)0.
    // Every real value is handled below, so the warning is safe to ignore here.
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
    /// Title and body used for the push message on the phone. Only AccessRequested and EmergencyAccess
    /// have their own push wording; every other type uses the same text as the list.
    /// For a guardian, the dependent's name is added to the title.
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
