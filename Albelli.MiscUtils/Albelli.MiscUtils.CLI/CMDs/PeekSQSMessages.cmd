@echo off
REM CALL PeekSQSMessages_worker.cmd 890564833321 Fulfillment-DeliveryTracking-Shipment-DLQ 1 15 "" "PlantCode,Source,X-Client-Name,X-CorrelationId,status" false -Debug
CALL PeekSQSMessages_worker.cmd 890564833321 Fulfillment-DeliveryTracking-Shipment-DLQ "" 15 "" "PlantCode,Source,X-Client-Name,X-CorrelationId,status" true -SkipLogo