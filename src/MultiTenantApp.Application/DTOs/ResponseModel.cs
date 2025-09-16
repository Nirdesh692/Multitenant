using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.DTOs
{
    public class ResponseModel<T>
    {
        public bool success { get; set; }
        public string errorMessage { get; set; }
        public string detailErrorMessage { get; set; }
        public T data { get; set; }
        public ResponseModel(bool Success, T Data, string ErrorMessage = null!, string DetailErrorMessage = null!)
        {
            success = Success;
            errorMessage = ErrorMessage;
            detailErrorMessage = DetailErrorMessage;
            data = Data;
        }
    }
}
