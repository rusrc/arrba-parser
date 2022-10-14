namespace Arrba.Parser.DbContext
{
    public enum Status
    {
        /// <summary>
        /// "Active" when added successfully and waiting to update 
        /// </summary>
        Active = 1,
        /// <summary>
        /// "Wait to check" when was thrown expected exception and wating to readd
        /// </summary>
        WaitToCheck = 2,
        /// <summary>
        /// "Not found" sold or returns not found status code
        /// </summary>
        NotFound = 3,
        /// <summary>
        /// Return error
        /// </summary>
        Error = 4
    }
}
