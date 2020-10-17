As part of the global meta-system, all items define how they affect 
sensory data. Most items will either allow sensory data to pass 
freely or block the data to some varying degree.

# Sense Data Propagation

A ``SensePropertiesMap`` holds information on how sensory information
spreads through the world. For light, this map collects translucency,
for heat it collects an insulation factor etc. The system precomputes 
the ``SensePropertiesMap`` from all items available on the map. 


# Sense Processing

The current system defines 4 common physical properties that can be
sensed:

* Light
* Heat
* Noise
* Smell

Processing of sensing information for actors is a two stage process.

Sense sources create signals in the world. Those signals are global effects
and are available to all actors that can potentially observe these signals. 
This means they are calculated once per frame, and can be heavily cached 
if the source is constant (does not change position or intensity).

Most sense types can be defined simply by the interaction between a sense
source and a sense receptor. A noise forms a relationship between the
noise source and the actor hearing the noise. 


