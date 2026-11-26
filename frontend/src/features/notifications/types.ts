export type NotificationKind =
  | "accepted" // patient approved the request
  | "not_accepted" // 48h passed with no reply
  | "ended" // active access was ended
  | "expired" // access period ended
  | "extended"; // patient extended the access

export type AppNotification = {
  id: string;
  kind: NotificationKind;
  patientName: string; // already masked
  /** epoch ms */
  createdAt: number;
  read: boolean;
};
