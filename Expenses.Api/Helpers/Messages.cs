namespace Expenses.Api.Helpers;

public static class Messages {
    public const string CONFLICT_MESSAGE_PATTERN = "{0} with value {1} {2} already exists.";
    public const string NOT_FOUND_MESSAGE_PATTERN = "{0} with value {1} {2} not found.";
    public const string BAD_REQUEST_EMPTY_FIELD = "Field {0} should be filled.";
    public const string BAD_REQUEST_LESSER_VALUE = "Field {0} should be higher than {1}.";
    public const string BAD_REQUEST_LENGTH_VALUE_BETWEEN = "Field {0} should have characters between {1} and {2}.";
    public const string BAD_REQUEST_LENGTH_VALUE_HIGHER_THAN = "Field {0} should have at least {1} characters.";
    public const string BAD_REQUEST_EMPTY_CONDITIONAL_FIELD = "Field {0} should be filled when {1} is filled with {2}.";
    public const string BAD_REQUEST_FILLED_CONDITIONAL_FIELD = "Field {0} should be empty when {1} is filled with {2}.";

}