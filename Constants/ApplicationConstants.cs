namespace FlightManagementSystem.Constants
{
    /// <summary>
    /// Application-wide named constants. Per SRS §5.4.3 and §5.4.14, all constant
    /// values must be defined here rather than as inline magic numbers/strings.
    /// </summary>
    public static class ApplicationConstants
    {
        // ── Roles ────────────────────────────────────────────────────────────────
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_STAFF = "Staff";
        public const string ROLE_PASSENGER = "Passenger";

        // ── Default Accounts (seeded) ─────────────────────────────────────────
        public const string DEFAULT_ADMIN_EMAIL = "admin@bermudatriangle.com";
        public const string DEFAULT_ADMIN_PASSWORD = "Admin@123";
        public const string DEFAULT_STAFF_EMAIL = "staff@bermudatriangle.com";
        public const string DEFAULT_STAFF_PASSWORD = "Staff@123";

        // ── Flight Status Values ──────────────────────────────────────────────
        public const string FLIGHT_STATUS_SCHEDULED = "Scheduled";
        public const string FLIGHT_STATUS_DELAYED = "Delayed";
        public const string FLIGHT_STATUS_CANCELLED = "Cancelled";
        public const string FLIGHT_STATUS_DEPARTED = "Departed";
        public const string FLIGHT_STATUS_ARRIVED = "Arrived";

        // ── Reservation Status Values ─────────────────────────────────────────
        public const string RESERVATION_STATUS_CONFIRMED = "Confirmed";
        public const string RESERVATION_STATUS_CANCELLED = "Cancelled";
        public const string RESERVATION_STATUS_PENDING = "Pending";

        // ── Seat Classes ──────────────────────────────────────────────────────
        public const string CLASS_ECONOMY = "Economy";
        public const string CLASS_BUSINESS = "Business";
        public const string CLASS_FIRST = "First";

        // ── Price Multipliers ─────────────────────────────────────────────────
        public const decimal PRICE_MULTIPLIER_ECONOMY = 1.0m;
        public const decimal PRICE_MULTIPLIER_BUSINESS = 2.0m;
        public const decimal PRICE_MULTIPLIER_FIRST = 3.5m;

        // ── Payment Methods ───────────────────────────────────────────────────
        public const string PAYMENT_METHOD_CREDIT_CARD = "Credit Card";
        public const string PAYMENT_METHOD_DEBIT_CARD = "Debit Card";
        public const string PAYMENT_METHOD_PAYPAL = "PayPal";
        public const string PAYMENT_METHOD_BANK_TRANSFER = "Bank Transfer";

        // ── Payment Status ────────────────────────────────────────────────────
        public const string PAYMENT_STATUS_COMPLETED = "Completed";
        public const string PAYMENT_STATUS_FAILED = "Failed";
        public const string PAYMENT_STATUS_PENDING = "Pending";

        // ── Notification Types ────────────────────────────────────────────────
        public const string NOTIFICATION_CONFIRMATION = "Confirmation";
        public const string NOTIFICATION_CANCELLATION = "Cancellation";
        public const string NOTIFICATION_FLIGHT_CHANGE = "FlightChange";
        public const string NOTIFICATION_SYSTEM_ALERT = "System";
        public const string NOTIFICATION_ERROR = "Error";

        // ── Activity / Audit Log Actions ──────────────────────────────────────
        public const string AUDIT_ACTION_LOGIN = "Login";
        public const string AUDIT_ACTION_LOGOUT = "Logout";
        public const string AUDIT_ACTION_LOGIN_FAILED = "LoginFailed";
        public const string AUDIT_ACTION_CREATE = "Create";
        public const string AUDIT_ACTION_UPDATE = "Update";
        public const string AUDIT_ACTION_DELETE = "Delete";
        public const string AUDIT_ACTION_CANCEL = "Cancel";
        public const string AUDIT_ACTION_PAYMENT = "Payment";
        public const string AUDIT_ACTION_BACKUP = "Backup";
        public const string AUDIT_ACTION_RESTORE = "Restore";
        public const string AUDIT_ACTION_ROLE_CHANGE = "RoleChange";
        public const string AUDIT_ACTION_PASSWORD_RESET = "PasswordReset";
        public const string AUDIT_ACTION_ACCOUNT_LOCKED = "AccountLocked";
        public const string AUDIT_ACTION_CONFIG_CHANGE = "ConfigChange";

        // ── System Setting Keys (REQ-65) ──────────────────────────────────────
        public const string SETTING_MAINTENANCE_MODE = "MaintenanceMode";
        public const string SETTING_MAX_LOGIN_ATTEMPTS = "MaxLoginAttempts";
        public const string SETTING_SESSION_TIMEOUT_MINUTES = "SessionTimeoutMinutes";
        public const string SETTING_LOCKOUT_DURATION_MINUTES = "LockoutDurationMinutes";
        public const string SETTING_BACKUP_FREQUENCY_HOURS = "BackupFrequencyHours";
        public const string SETTING_MIN_PASSWORD_LENGTH = "MinPasswordLength";
        public const string SETTING_REQUIRE_STRONG_PASSWORD = "RequireStrongPassword";
        public const string SETTING_NOTIFICATION_EMAIL_ENABLED = "NotificationEmailEnabled";
        public const string SETTING_SMTP_HOST = "SmtpHost";
        public const string SETTING_SMTP_PORT = "SmtpPort";

        // ── Performance Thresholds (SRS §5.1) ────────────────────────────────
        public const int MAX_RESPONSE_TIME_SECONDS = 3;
        public const int MAX_PAYMENT_PROCESSING_SECONDS = 30;

        // ── Backup ────────────────────────────────────────────────────────────
        public const string BACKUP_STATUS_SUCCESS = "Success";
        public const string BACKUP_STATUS_FAILED = "Failed";
        public const string BACKUP_STATUS_IN_PROGRESS = "InProgress";

        // ── Pagination ────────────────────────────────────────────────────────
        public const int DEFAULT_PAGE_SIZE = 20;
        public const int DASHBOARD_RECENT_ITEMS = 5;
        public const int MAX_SEARCH_RESULTS = 100;

        // ── Seat generation bounds ────────────────────────────────────────────
        public const int SEAT_ROW_MIN = 1;
        public const int SEAT_ROW_MAX = 40;
        public const int SEAT_COLUMN_COUNT = 6;
    }
}
