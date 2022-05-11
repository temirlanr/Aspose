# ImageProcessor

Image Processor which compares two same images for differences

#### Completed Test Task for Aspose

## Installation

I haven't really published this project, so the easiest way is to clone the repo or just copy-paste the code. Documentation is included in the code.

## Design Approach

This project was made using some kind of Builder design pattern.
Regarding the algorithm, it is pretty straightforward: 
* The differences are detected by comparing the pixels. There are 2 comparison modes for choice at the moment: ARGB and RGB, but it is easily scalable, so you can add your own algorithm by adding it to enum ComparisonAlgorithm and then specifying the formula in diffFormula Dictionary.
* The 2D array of booleans is created with location of different pixels.
* At this point it looked like a popular Number of Islands problem, but I needed to cluster those areas with detected differences in pixels.
* I created a PixelCluster object for better working with those areas.
* Recursive Depth First Search algorithm is implemented with clustering pixel locations into PixelCluster objects.
* PixelCluster objects returned to main method that writes all the info about those clusters and saves 2 images into local drive that have differences surrounded by red rectangles.
* There are to modes: synchronous and asynchronous, so you can choose. For better performance I recommend the asynchronous. Also asynchronous mode works with IAsyncEnumerable, which outputs PixelCluster objects "on the fly", meaning you see info about them as soon as they are detected.

## API Reference and Documentation is provided within the code

### Strong features

* Easy scalability
* Customization
* Understandable code
* Fairly quick algorithms used

### Weak features

* Exception handling. For example, the Stack Overflow can easily happen when not configured properly
* Tests didn't cover all of the functionality
* Works only on Windows

## Possible ways to improve

It was my first project on image processing, so it was, indeed, very hard and interesting to work on it. Also, I understand that there probably are many mistakes that an experienced developer would easily point out. I, myself, could point out these:
* Better implementation of asynchronous methods
* Better design
* Research on how to work with images, colors, color spaces and files in general
* Think of an algorithm that wouldn't use that much space
* Exception handling and proper validation
* Publish it as a Library
