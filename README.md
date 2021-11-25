# Auto Refresh Rate Changer Worker
A Service Worker to change the refresh rate of your monitor when changing between power schemes regardless of the AC power state.

## Why?
Well, in my country we have constant power outages, so during a power outage, my laptop (with a 144hz monitor) would be running on an external battery.

Most OEMs provide this functionality with their laptops, but the trigger would be the loss of AC power which isn't what i want since technically even during a power outage i am still on AC power.

So this worker switches the refresh rate on the basis of the chosen power scheme rather than the detection of AC power.

## How?
Most of the code is done using PInvoke to detect the active scheme, detect video mode and set the new frequency when necessary.

The worker uses the `appsettings.json` config file (which should be filled by you) to get what are the power schemes that you consider high performance and what frequency limits you would like to switch between.

## Todo
- Let the worker get the highest and the lowest refresh rates and switch between them or let the user set the limits in the config file.
- Maybe find a way to get the family of a scheme automatically? like: `Power saver` `High performance` `Balanced`.
