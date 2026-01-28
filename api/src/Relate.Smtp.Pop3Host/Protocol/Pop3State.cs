namespace Relate.Smtp.Pop3Host.Protocol;

public enum Pop3State
{
    Authorization,  // USER/PASS only
    Transaction,    // STAT/LIST/RETR/DELE/NOOP/RSET
    Update          // QUIT - apply deletions
}
