﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace WindowsService1
{
    [ServiceContract]
    public interface IHelloWorldService
    {
        [OperationContract]
        [WebGet]
        string SayHello();
    }

    public class HelloWorldService : IHelloWorldService
    {
        public string SayHello()
        {
            return "hello world";
        }
    }
}
