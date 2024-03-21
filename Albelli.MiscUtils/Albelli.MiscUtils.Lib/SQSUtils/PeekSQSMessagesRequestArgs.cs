using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.SQSUtils
{
    public class PeekSQSMessagesRequestArgs
    {
        public PeekSQSMessagesRequestArgs()
        {
            nrOfMsgs = 10;
            visibilityTimeout = 20;
        }
        public RegionEndpoint awsRegion { get; set; }
        public string awsAccId { get; set; }
        public string queueName { get; set; }
        public int nrOfMsgs { get; set; }
        public int visibilityTimeout { get; set; }
        public List<string> attrNames { get; set; }
        public List<string> msgAttrNames { get; set; }
        public bool entireQueue { get; set; }
    }
}
