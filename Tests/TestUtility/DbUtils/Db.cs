namespace TestUtility.DbUtils;

public static class Db
{
    public static string PostgresConnectionStr { get; private set; } = string.Empty;
    public static string SqlServerConnectionStr { get; private set; } = string.Empty;

    public static void SetPostgresConnectionStr(string connectionStr)
    {
        PostgresConnectionStr = connectionStr;
        Fetch = new Fetch(PostgresConnectionStr, SqlServerConnectionStr);
        Insert = new Insert(PostgresConnectionStr, SqlServerConnectionStr);
    }

    public static void SetSqlServerConnectionStr(string connectionStr)
    {
        SqlServerConnectionStr = connectionStr;
        Fetch = new Fetch(PostgresConnectionStr, SqlServerConnectionStr);
        Insert = new Insert(PostgresConnectionStr, SqlServerConnectionStr);
    }

    public static IFetch Fetch { get; private set; } = new Fetch(PostgresConnectionStr, SqlServerConnectionStr);
    public static IInsert Insert { get; private set; } = new Insert(PostgresConnectionStr, SqlServerConnectionStr);
}