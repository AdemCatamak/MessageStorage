using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.DataAccessLayer.QueryBuilders
{
    public interface IJobQueryBuilder : IQueryBuilder<Job>
    {
        (string, IEnumerable<IDbDataParameter>) SetFirstWaitingJobToInProgress();
        (string, IEnumerable<IDbDataParameter>) Update(Job job);
        (string, IEnumerable<IDbDataParameter>) GetJobCountByStatus(JobStatuses jobStatus);
        IEnumerable<Job> MapData(DataRowCollection rows);
    }
}