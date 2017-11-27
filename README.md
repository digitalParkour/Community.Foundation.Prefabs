Sitecore - Prefabs
==============

Summary
--------------
Design Sitecore pages more quickly and consistently by saving work to a prefabs library.

This module allows you to:  
1 > select a section of a page you like and hit a "Make Prefab" button.  
    ... this gets saved to a prefab library, then  
2 > add prefab, just like any other rendering, to any page  
    ... in fact any language, version, device, placeholder path, etc

This was built and tested with Sitecore 8.2 update 5, MVC only. There is a general release and an SXA supported release.

It is designed with pipelines and IoC to ease extension and tailoring to your solution.
  
Setup
--------------
Either:
* Install Sitecore package:  
	\releases\Community.Foundation.Prefabs-*.zip (for general release)  
	OR  
	\releases\Community.Foundation.Prefabs-*.SXA.zip (for sxa supported release)  
		- Same as general with only two differences  
			> Foundation.Prefabs.Sxa.config is enabled  
			> "Prefab" data template inherits "_SXA support" template for Experience Editor compatibility (SXA rendering parameter options don't work unless item inherits SXA page)
		
Or:
1. Include this project in your Helix style solution
2. Update NuGet references to match target sitecore version
3. Install sitecore package for data or enable Foundation.Prefabs.Serialization.config and sync unicorn
