As part of the global meta-system, all items define how they affect 
sensory data. Most items will either allow sensory data to pass 
freely or block the data to some varying degree.

The system precomputes a SenseProperties map from all items available.



Processing of sensing information for actors is a two stage process.

Sense sources create signals in the world. Those signals are global effects
and are available to all actors that can observe these signals. This means
they are calculated once per frame, and can be heavily cached if the source
is constant (ie static lights).

This creates a set of sense-data-maps (brightness, smells, heat/cold, noise)
that are refreshed every frame.

Actors have sensors (means to read the sense-data-maps). Sensors have a 
detection radius and sensitivity (an activation threshold). Each sensor
maintains a detection filter (a mapping that defines the sensitity for 
each cell within the radius). The filter is computed for each actor and
each sense.

Sensing is assumed to be bi-directional. If I can see you, you can see me
(assuming same vision capabilities). 

To check whether an actor can perceive a given tile, the actor first checks
the sense-detection-filter. This returns the detection sensitivity at the given 
point as percentage (0 = cannot see anything, 1 = can see everything).
The detection sensivity is then combined with the actual sense-data-map to
calculate the sense-input.


