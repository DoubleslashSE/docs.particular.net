---
title: Individualizing endpoint queue names when Scaling-Out
summary: How to enable individualizing queue names when Scaling-Out endpoints
tags: 
- Scale Out
---

INFO: This is relevant to versions 5.2 and above.

When scaling out an endpoint you need to make sure it gets a unique input queue per instance while keeping the endpoint name the same.
For this reason we have introduced a new API from NServiceBus v5.2 that allows you to enable queue name individualization per endpoint.

<!-- import UniqueQueuePerEndpointInstance --> 
This will tell NServiceBus to use the suffix registered by the transport or host to make sure that each instance of your endpoint will be unique. For MSMQ this is a no-op since you commonly scaled them out by running multiple instances on different machines and that will give them a unique address out of the box. Eg: `sales@server1, sales@serverN` etc.

For broker transports that's no longer true, hence the need for a suffix. For on-premise operations the machine name is likely to be used and in cloud scenarios like on Azure the instance id is better suited since machines will dynamically change. 

RabbitMQ example:  `sales-server1, sales-serverN` 
Azure ServiceBus example:  `sales-1, sales-N` where N is the role instance id

If you need full control over the suffix or if the transport hasn't registered a default you can control it using:
<!-- import UniqueQueuePerEndpointInstanceWithSuffix --> 