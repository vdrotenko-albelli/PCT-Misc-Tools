@echo off
CALL DLQMsgsAnalyze_worker.cmd F:\home\vmdrot\EPAM\Projs\PTBX-NDC\JIRA\PCT-11198\Fulfillment-DeliveryTracking-Shipment-DLQ.msgs.json "MessageId,MessageAttributes.X-CorrelationId.StringValue"