namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a SalesForce error status code.
    /// </summary>
    public enum StatusCode
    {

        /// <summary>
        /// All or none operation rolled back.
        /// </summary>
        ALL_OR_NONE_OPERATION_ROLLED_BACK,

        /// <summary>
        /// Already in process.
        /// </summary>
        ALREADY_IN_PROCESS,

        /// <summary>
        /// Assignee type required.
        /// </summary>
        ASSIGNEE_TYPE_REQUIRED,

        /// <summary>
        /// Bad custom entity parent domain.
        /// </summary>
        BAD_CUSTOM_ENTITY_PARENT_DOMAIN,

        /// <summary>
        /// BCC not allowed if BCC compliance enabled.
        /// </summary>
        BCC_NOT_ALLOWED_IF_BCC_COMPLIANCE_ENABLED,

        /// <summary>
        /// Cannot cascade product active.
        /// </summary>
        CANNOT_CASCADE_PRODUCT_ACTIVE,

        /// <summary>
        /// Cannot change field type of apex referenced field.
        /// </summary>
        CANNOT_CHANGE_FIELD_TYPE_OF_APEX_REFERENCED_FIELD,

        /// <summary>
        /// Cannot create another managed package.
        /// </summary>
        CANNOT_CREATE_ANOTHER_MANAGED_PACKAGE,

        /// <summary>
        /// Cannot deactivate division.
        /// </summary>
        CANNOT_DEACTIVATE_DIVISION,

        /// <summary>
        /// Cannot delete last dated conversion rate.
        /// </summary>
        CANNOT_DELETE_LAST_DATED_CONVERSION_RATE,

        /// <summary>
        /// Cannot delete managed object.
        /// </summary>
        CANNOT_DELETE_MANAGED_OBJECT,

        /// <summary>
        /// Cannot disable last admin.
        /// </summary>
        CANNOT_DISABLE_LAST_ADMIN,

        /// <summary>
        /// Cannot enable IP restrict requests.
        /// </summary>
        CANNOT_ENABLE_IP_RESTRICT_REQUESTS,

        /// <summary>
        /// Cannot insert update activate entity.
        /// </summary>
        CANNOT_INSERT_UPDATE_ACTIVATE_ENTITY,

        /// <summary>
        /// Cannot modify managed object.
        /// </summary>
        CANNOT_MODIFY_MANAGED_OBJECT,

        /// <summary>
        /// Cannot rename apex referenced field.
        /// </summary>
        CANNOT_RENAME_APEX_REFERENCED_FIELD,

        /// <summary>
        /// Cannot rename apex referenced object.
        /// </summary>
        CANNOT_RENAME_APEX_REFERENCED_OBJECT,

        /// <summary>
        /// Cannot reparent record.
        /// </summary>
        CANNOT_REPARENT_RECORD,

        /// <summary>
        /// Cannot update converted lead.
        /// </summary>
        CANNOT_UPDATE_CONVERTED_LEAD,

        /// <summary>
        /// Cannot disable corp currency.
        /// </summary>
        CANT_DISABLE_CORP_CURRENCY,

        /// <summary>
        /// Cannot unset corp currency.
        /// </summary>
        CANT_UNSET_CORP_CURRENCY,

        /// <summary>
        /// Child share fails parent.
        /// </summary>
        CHILD_SHARE_FAILS_PARENT,

        /// <summary>
        /// Circular dependency.
        /// </summary>
        CIRCULAR_DEPENDENCY,

        /// <summary>
        /// Community not accessible.
        /// </summary>
        COMMUNITY_NOT_ACCESSIBLE,

        /// <summary>
        /// Custom CLOB field limit exceeded.
        /// </summary>
        CUSTOM_CLOB_FIELD_LIMIT_EXCEEDED,

        /// <summary>
        /// Custom entity or field limit.
        /// </summary>
        CUSTOM_ENTITY_OR_FIELD_LIMIT,

        /// <summary>
        /// Custom field index limit exceeded.
        /// </summary>
        CUSTOM_FIELD_INDEX_LIMIT_EXCEEDED,

        /// <summary>
        /// Custom index exists.
        /// </summary>
        CUSTOM_INDEX_EXISTS,

        /// <summary>
        /// Custom link limit exceeded.
        /// </summary>
        CUSTOM_LINK_LIMIT_EXCEEDED,

        /// <summary>
        /// Custom tab limit exceeded.
        /// </summary>
        CUSTOM_TAB_LIMIT_EXCEEDED,

        /// <summary>
        /// Delete failed.
        /// </summary>
        DELETE_FAILED,

        /// <summary>
        /// Delete required on cascade.
        /// </summary>
        DELETE_REQUIRED_ON_CASCADE,

        /// <summary>
        /// Dependency exists.
        /// </summary>
        DEPENDENCY_EXISTS,

        /// <summary>
        /// Duplicate case solution.
        /// </summary>
        DUPLICATE_CASE_SOLUTION,

        /// <summary>
        /// Duplicate comm. nickname.
        /// </summary>
        DUPLICATE_COMM_NICKNAME,

        /// <summary>
        /// Duplicate custom entity definition.
        /// </summary>
        DUPLICATE_CUSTOM_ENTITY_DEFINITION,

        /// <summary>
        /// Duplicated custom tab motif.
        /// </summary>
        DUPLICATE_CUSTOM_TAB_MOTIF,

        /// <summary>
        /// Duplicated developer name.
        /// </summary>
        DUPLICATE_DEVELOPER_NAME,

        /// <summary>
        /// Duplicated external id.
        /// </summary>
        DUPLICATE_EXTERNAL_ID,

        /// <summary>
        /// Duplicated master label.
        /// </summary>
        DUPLICATE_MASTER_LABEL,

        /// <summary>
        /// Duplicated sender display name.
        /// </summary>
        DUPLICATE_SENDER_DISPLAY_NAME,

        /// <summary>
        /// Duplicated username.
        /// </summary>
        DUPLICATE_USERNAME,

        /// <summary>
        /// Duplicated value.
        /// </summary>
        DUPLICATE_VALUE,

        /// <summary>
        /// E-mail not processed due to prior error.
        /// </summary>
        EMAIL_NOT_PROCESSED_DUE_TO_PRIOR_ERROR,

        /// <summary>
        /// Empty SControl file name.
        /// </summary>
        EMPTY_SCONTROL_FILE_NAME,

        /// <summary>
        /// Entity failed IfLastModified on update.
        /// </summary>
        ENTITY_FAILED_IFLASTMODIFIED_ON_UPDATE,

        /// <summary>
        /// Entity is archived.
        /// </summary>
        ENTITY_IS_ARCHIVED,

        /// <summary>
        /// Entity is deleted.
        /// </summary>
        ENTITY_IS_DELETED,

        /// <summary>
        /// Entity is locked.
        /// </summary>
        ENTITY_IS_LOCKED,

        /// <summary>
        /// Error in mailer.
        /// </summary>
        ERROR_IN_MAILER,

        /// <summary>
        /// Failed activation.
        /// </summary>
        FAILED_ACTIVATION,

        /// <summary>
        /// Field custom validation exception.
        /// </summary>
        FIELD_CUSTOM_VALIDATION_EXCEPTION,

        /// <summary>
        /// Field filter validation exception.
        /// </summary>
        FIELD_FILTER_VALIDATION_EXCEPTION,

        /// <summary>
        /// Field integrity exception.
        /// </summary>
        FIELD_INTEGRITY_EXCEPTION,

        /// <summary>
        /// Filtered lookup limit exceeded.
        /// </summary>
        FILTERED_LOOKUP_LIMIT_EXCEEDED,

        /// <summary>
        /// HTML file upload not allowed.
        /// </summary>
        HTML_FILE_UPLOAD_NOT_ALLOWED,

        /// <summary>
        /// Image too large.
        /// </summary>
        IMAGE_TOO_LARGE,

        /// <summary>
        /// Inactive owner or user.
        /// </summary>
        INACTIVE_OWNER_OR_USER,

        /// <summary>
        /// Insufficient access on cross reference entity.
        /// </summary>
        INSUFFICIENT_ACCESS_ON_CROSS_REFERENCE_ENTITY,

        /// <summary>
        /// Insufficient access or readonly.
        /// </summary>
        INSUFFICIENT_ACCESS_OR_READONLY,

        /// <summary>
        /// Invalid access level.
        /// </summary>
        INVALID_ACCESS_LEVEL,

        /// <summary>
        /// Invalid argument type.
        /// </summary>
        INVALID_ARGUMENT_TYPE,

        /// <summary>
        /// Invalid assignee type.
        /// </summary>
        INVALID_ASSIGNEE_TYPE,

        /// <summary>
        /// Invalid assignment rule.
        /// </summary>
        INVALID_ASSIGNMENT_RULE,

        /// <summary>
        /// Invalid batch operation.
        /// </summary>
        INVALID_BATCH_OPERATION,

        /// <summary>
        /// Invalid content type.
        /// </summary>
        INVALID_CONTENT_TYPE,

        /// <summary>
        /// Invalid credit card info.
        /// </summary>
        INVALID_CREDIT_CARD_INFO,

        /// <summary>
        /// Invalid cross reference key.
        /// </summary>
        INVALID_CROSS_REFERENCE_KEY,

        /// <summary>
        /// Invalid cross reference type for field.
        /// </summary>
        INVALID_CROSS_REFERENCE_TYPE_FOR_FIELD,

        /// <summary>
        /// Invalid currency conv. rate.
        /// </summary>
        INVALID_CURRENCY_CONV_RATE,

        /// <summary>
        /// Invalid currency corp. rate.
        /// </summary>
        INVALID_CURRENCY_CORP_RATE,

        /// <summary>
        /// Invalid currency ISO.
        /// </summary>
        INVALID_CURRENCY_ISO,

        /// <summary>
        /// Invalid data category group reference.
        /// </summary>
        INVALID_DATA_CATEGORY_GROUP_REFERENCE,

        /// <summary>
        /// Invalid data URI.
        /// </summary>
        INVALID_DATA_URI,

        /// <summary>
        /// Invalid e-mail address.
        /// </summary>
        INVALID_EMAIL_ADDRESS,

        /// <summary>
        /// Invalid empty key owner.
        /// </summary>
        INVALID_EMPTY_KEY_OWNER,

        /// <summary>
        /// Invalid field.
        /// </summary>
        INVALID_FIELD,

        /// <summary>
        /// Invalid field for insert update.
        /// </summary>
        INVALID_FIELD_FOR_INSERT_UPDATE,

        /// <summary>
        /// Invalid field when using template.
        /// </summary>
        INVALID_FIELD_WHEN_USING_TEMPLATE,

        /// <summary>
        /// Invalid filter action.
        /// </summary>
        INVALID_FILTER_ACTION,

        /// <summary>
        /// Invalid Google docs URL.
        /// </summary>
        INVALID_GOOGLE_DOCS_URL,

        /// <summary>
        /// Invalid id field.
        /// </summary>
        INVALID_ID_FIELD,

        /// <summary>
        /// Invalid inet address.
        /// </summary>
        INVALID_INET_ADDRESS,

        /// <summary>
        /// Invalid line item clone state.
        /// </summary>
        INVALID_LINEITEM_CLONE_STATE,

        /// <summary>
        /// Invalid master or translated solution.
        /// </summary>
        INVALID_MASTER_OR_TRANSLATED_SOLUTION,

        /// <summary>
        /// Invalid message id reference.
        /// </summary>
        INVALID_MESSAGE_ID_REFERENCE,

        /// <summary>
        /// Invalid operation.
        /// </summary>
        INVALID_OPERATION,

        /// <summary>
        /// Invalid operator.
        /// </summary>
        INVALID_OPERATOR,

        /// <summary>
        /// Invalid or null for restricted picklist.
        /// </summary>
        INVALID_OR_NULL_FOR_RESTRICTED_PICKLIST,

        /// <summary>
        /// Invalid partner network status.
        /// </summary>
        INVALID_PARTNER_NETWORK_STATUS,

        /// <summary>
        /// Invalid person account operation.
        /// </summary>
        INVALID_PERSON_ACCOUNT_OPERATION,

        /// <summary>
        /// Invalid read only user DML.
        /// </summary>
        INVALID_READ_ONLY_USER_DML,

        /// <summary>
        /// Invalid save as activity flag.
        /// </summary>
        INVALID_SAVE_AS_ACTIVITY_FLAG,

        /// <summary>
        /// Invalid session id.
        /// </summary>
        INVALID_SESSION_ID,

        /// <summary>
        /// Invalid setup owner.
        /// </summary>
        INVALID_SETUP_OWNER,

        /// <summary>
        /// Invalid status.
        /// </summary>
        INVALID_STATUS,

        /// <summary>
        /// Invalid type.
        /// </summary>
        INVALID_TYPE,

        /// <summary>
        /// Invalid type for operation.
        /// </summary>
        INVALID_TYPE_FOR_OPERATION,

        /// <summary>
        /// Invalid type on field in record.
        /// </summary>
        INVALID_TYPE_ON_FIELD_IN_RECORD,

        /// <summary>
        /// IP range limit exceeded.
        /// </summary>
        IP_RANGE_LIMIT_EXCEEDED,

        /// <summary>
        /// License limit exceeded.
        /// </summary>
        LICENSE_LIMIT_EXCEEDED,

        /// <summary>
        /// Light portal user exception.
        /// </summary>
        LIGHT_PORTAL_USER_EXCEPTION,

        /// <summary>
        /// Limit exceeded.
        /// </summary>
        LIMIT_EXCEEDED,

        /// <summary>
        /// Malformed id.
        /// </summary>
        MALFORMED_ID,

        /// <summary>
        /// Manager not defined.
        /// </summary>
        MANAGER_NOT_DEFINED,

        /// <summary>
        /// Mass mail retry limit exceeded.
        /// </summary>
        MASSMAIL_RETRY_LIMIT_EXCEEDED,

        /// <summary>
        /// Mass mail limit exceeded.
        /// </summary>
        MASS_MAIL_LIMIT_EXCEEDED,

        /// <summary>
        /// Maximum cc emails exceeded.
        /// </summary>
        MAXIMUM_CCEMAILS_EXCEEDED,

        /// <summary>
        /// Maximum dashboard components exceeded.
        /// </summary>
        MAXIMUM_DASHBOARD_COMPONENTS_EXCEEDED,

        /// <summary>
        /// Maximum hierarchy levels reached.
        /// </summary>
        MAXIMUM_HIERARCHY_LEVELS_REACHED,

        /// <summary>
        /// Maximum size of attachment.
        /// </summary>
        MAXIMUM_SIZE_OF_ATTACHMENT,

        /// <summary>
        /// Maximum size of document.
        /// </summary>
        MAXIMUM_SIZE_OF_DOCUMENT,

        /// <summary>
        /// Max actions per rule exceeded.
        /// </summary>
        MAX_ACTIONS_PER_RULE_EXCEEDED,

        /// <summary>
        /// Max active rules exceeded.
        /// </summary>
        MAX_ACTIVE_RULES_EXCEEDED,

        /// <summary>
        /// Max approval steps exceeded.
        /// </summary>
        MAX_APPROVAL_STEPS_EXCEEDED,

        /// <summary>
        /// Max formulas per rule exceeded.
        /// </summary>
        MAX_FORMULAS_PER_RULE_EXCEEDED,

        /// <summary>
        /// Max rules exceeded.
        /// </summary>
        MAX_RULES_EXCEEDED,

        /// <summary>
        /// Max rule entries exceeded.
        /// </summary>
        MAX_RULE_ENTRIES_EXCEEDED,

        /// <summary>
        /// Max task description exceeeded.
        /// </summary>
        MAX_TASK_DESCRIPTION_EXCEEEDED,

        /// <summary>
        /// Max tm. rules exceeded.
        /// </summary>
        MAX_TM_RULES_EXCEEDED,

        /// <summary>
        /// Max tm. rule items exceeded.
        /// </summary>
        MAX_TM_RULE_ITEMS_EXCEEDED,

        /// <summary>
        /// Merge failed.
        /// </summary>
        MERGE_FAILED,

        /// <summary>
        /// Missing argument.
        /// </summary>
        MISSING_ARGUMENT,

        /// <summary>
        /// Mixed DML operation.
        /// </summary>
        MIXED_DML_OPERATION,

        /// <summary>
        /// Nonunique shipping address.
        /// </summary>
        NONUNIQUE_SHIPPING_ADDRESS,

        /// <summary>
        /// No applicable process.
        /// </summary>
        NO_APPLICABLE_PROCESS,

        /// <summary>
        /// No attachment permission.
        /// </summary>
        NO_ATTACHMENT_PERMISSION,

        /// <summary>
        /// No inactive division members.
        /// </summary>
        NO_INACTIVE_DIVISION_MEMBERS,

        /// <summary>
        /// No mass mail permission.
        /// </summary>
        NO_MASS_MAIL_PERMISSION,

        /// <summary>
        /// Number outside valid range.
        /// </summary>
        NUMBER_OUTSIDE_VALID_RANGE,

        /// <summary>
        /// Number of history fields by sobject exceeded.
        /// </summary>
        NUM_HISTORY_FIELDS_BY_SOBJECT_EXCEEDED,

        /// <summary>
        /// Opted out of mass mail.
        /// </summary>
        OPTED_OUT_OF_MASS_MAIL,

        /// <summary>
        /// Operation with invalid user type exception.
        /// </summary>
        OP_WITH_INVALID_USER_TYPE_EXCEPTION,

        /// <summary>
        /// Package license required.
        /// </summary>
        PACKAGE_LICENSE_REQUIRED,

        /// <summary>
        /// Portal no access.
        /// </summary>
        PORTAL_NO_ACCESS,

        /// <summary>
        /// Portal user already exists for contact.
        /// </summary>
        PORTAL_USER_ALREADY_EXISTS_FOR_CONTACT,

        /// <summary>
        /// Private contact on asset.
        /// </summary>
        PRIVATE_CONTACT_ON_ASSET,

        /// <summary>
        /// Record in use by workflow.
        /// </summary>
        RECORD_IN_USE_BY_WORKFLOW,

        /// <summary>
        /// Request running too long.
        /// </summary>
        REQUEST_RUNNING_TOO_LONG,

        /// <summary>
        /// Required feature missing.
        /// </summary>
        REQUIRED_FEATURE_MISSING,

        /// <summary>
        /// Required field missing.
        /// </summary>
        REQUIRED_FIELD_MISSING,

        /// <summary>
        /// Self reference from trigger.
        /// </summary>
        SELF_REFERENCE_FROM_TRIGGER,

        /// <summary>
        /// Share needed for child owner.
        /// </summary>
        SHARE_NEEDED_FOR_CHILD_OWNER,

        /// <summary>
        /// Single e-mail limit exceeded.
        /// </summary>
        SINGLE_EMAIL_LIMIT_EXCEEDED,

        /// <summary>
        /// Standard price not defined.
        /// </summary>
        STANDARD_PRICE_NOT_DEFINED,

        /// <summary>
        /// Storage limit exceeded.
        /// </summary>
        STORAGE_LIMIT_EXCEEDED,

        /// <summary>
        /// String too long.
        /// </summary>
        STRING_TOO_LONG,

        /// <summary>
        /// Tabset limit exceeded.
        /// </summary>
        TABSET_LIMIT_EXCEEDED,

        /// <summary>
        /// Template not active.
        /// </summary>
        TEMPLATE_NOT_ACTIVE,

        /// <summary>
        /// Territory realign in progress.
        /// </summary>
        TERRITORY_REALIGN_IN_PROGRESS,

        /// <summary>
        /// Text data outside supported charset.
        /// </summary>
        TEXT_DATA_OUTSIDE_SUPPORTED_CHARSET,

        /// <summary>
        /// Too many APEX requests.
        /// </summary>
        TOO_MANY_APEX_REQUESTS,

        /// <summary>
        /// Too many enum values.
        /// </summary>
        TOO_MANY_ENUM_VALUE,

        /// <summary>
        /// Transfer requires read.
        /// </summary>
        TRANSFER_REQUIRES_READ,

        /// <summary>
        /// Unable to lock row.
        /// </summary>
        UNABLE_TO_LOCK_ROW,

        /// <summary>
        /// Unavailable record type exception.
        /// </summary>
        UNAVAILABLE_RECORDTYPE_EXCEPTION,

        /// <summary>
        /// Undelete failed.
        /// </summary>
        UNDELETE_FAILED,

        /// <summary>
        /// Unknown exception.
        /// </summary>
        UNKNOWN_EXCEPTION,

        /// <summary>
        /// Unspecified e-mail address.
        /// </summary>
        UNSPECIFIED_EMAIL_ADDRESS,

        /// <summary>
        /// Unsupported APEX trigger operaton.
        /// </summary>
        UNSUPPORTED_APEX_TRIGGER_OPERATON,

        /// <summary>
        /// Unverified sender address.
        /// </summary>
        UNVERIFIED_SENDER_ADDRESS,

        /// <summary>
        /// User owns portal account exception.
        /// </summary>
        USER_OWNS_PORTAL_ACCOUNT_EXCEPTION,

        /// <summary>
        /// User with APEX shares exception.
        /// </summary>
        USER_WITH_APEX_SHARES_EXCEPTION,

        /// <summary>
        /// Weblink size limit exceeded.
        /// </summary>
        WEBLINK_SIZE_LIMIT_EXCEEDED,

        /// <summary>
        /// Wrong controller type.
        /// </summary>
        WRONG_CONTROLLER_TYPE

    }

}