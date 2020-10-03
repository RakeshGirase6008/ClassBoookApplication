﻿using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Common;
using System;

namespace ClassBookApplication.Service
{
    public class LogsService
    {
        #region Fields

        private readonly ClassBookLogsContext _context;


        #endregion

        #region Ctor
        public LogsService(ClassBookLogsContext context)
        {
            this._context = context;
        }

        #endregion

        #region Common

        /// <summary>
        /// Insert the Logs for exception
        /// </summary>
        public void InsertLogs(string moduleName, string shortMessage, string fullMessage, string APIName, int userId)
        {
            Logs logs = new Logs();
            logs.ModuleName = moduleName;
            logs.ShortMessage = shortMessage;
            logs.FullMessage = fullMessage;
            logs.APIName = APIName;
            logs.UserId = userId;
            logs.CreatedOnDate = DateTime.Now;
            _context.Logs.Add(logs);
            _context.SaveChanges();
        }

        /// <summary>
        /// Insert the Logs for exception
        /// </summary>
        public void InsertLogs(string moduleName, Exception exception, string APIName, int userId)
        {
            Logs logs = new Logs();
            logs.ModuleName = moduleName;
            logs.ShortMessage = exception.Message;
            logs.FullMessage = exception.InnerException?.Message;
            logs.APIName = APIName;
            logs.UserId = userId;
            logs.CreatedOnDate = DateTime.Now;
            _context.Logs.Add(logs); 
            _context.SaveChanges();
        }


        #endregion
    }
}
