# OpenTracing-LocalTracers
Wrap your existing tracer with the ability to log to a local file and/or the Console.

## Commonly used types
* `TracingConfigurationSection`
* `ColoredConsoleTracerDecorationFactory`
* `FileTracerDecorationFactory`

## Example outputs

### Console
Created with dataSerialization.SetTag='Simple' and dataSerialization.Log='Simple'
![Console Output Example](https://raw.githubusercontent.com/ndrwrbgs/OpenTracing-LocalTracers/master/img/ConsoleOutputImage.png)

### File
```csv
2019-03-31T14:16:20.1167750-07:00,2,Start,test
2019-03-31T14:16:20.1247813-07:00,3,Tag,foo,bar
2019-03-31T14:16:20.1257770-07:00,3,Start,inner
2019-03-31T14:16:20.1267771-07:00,3,Finish
2019-03-31T14:16:20.1277758-07:00,2,Finish
2019-03-31T14:16:48.9195913-07:00,2,Start,test
2019-03-31T14:16:48.9255545-07:00,3,Tag,foo,"bar,
hi"
2019-03-31T14:16:48.9255545-07:00,3,Start,inner
2019-03-31T14:16:48.9275536-07:00,3,Finish
2019-03-31T14:16:48.9275536-07:00,2,Finish
```

## Usage
1. Configure your tracers as mentioned in 'Configuration'.
1. Obtain your underlying tracer
1. Follow example:
```C#
ITracer GetConfiguredTracer(
    ConsoleElement consoleConfig,
    FileElement fileConfig,
    ITracer yourUnderlyingTracer)
{
    return yourUnderlyingTracer
        .Decorate(ColoredConsoleTracerDecorationFactory.Create(consoleConfig))
        .Decorate(FileTracerDecorationFactory.Create(fileConfig));
}
```

## Configuration

### File Configuration
Example ([latest](https://github.com/ndrwrbgs/OpenTracing-LocalTracers/blob/master/src/TestApp/App.config#L44-L50))
```xml
    <file>
      <enabled>true</enabled>
      <!-- If createIfNotExists is false, and the path does not exist, no file tracing will be done (as opposed to throwing an exception) -->
      <rootLocation createIfNotExists="true">
        <path>c:\Traces</path>
      </rootLocation>
      <!-- Only Csv is supported right now -->
      <outputMode>CSV</outputMode>
    </file>
```

### Console Configuration
Example ([latest](https://github.com/ndrwrbgs/OpenTracing-LocalTracers/blob/master/src/TestApp/App.config#L10-L42))
```xml
    <console>
      <enabled>true</enabled>
      <!-- Only BasedOnCategory is supported right now -->
      <colorMode>BasedOnCategory</colorMode>

      <colorsForBasedOnCategoryColorMode>
        <!-- available: Activated, Finished, SetTag, Log -->
        <!-- available: any ConsoleColor -->
        <Activated>Green</Activated>
        <Finished>Red</Finished>
        <SetTag>Magenta</SetTag>
        <Log>Blue</Log>
      </colorsForBasedOnCategoryColorMode>

      <!-- [4:21:01 pm] | 32   | StartSpan  | Whatever text is configured in the method -->
      <!-- available:
        date<colon><format>
        spanId
        spanIdFloatPadding
        logCategory
        logCategoryPadding
        outputData
      -->
      <format>[{date:h:mm:ss tt}] {spanId}{spanIdFloatPadding} | {logCategory}{logCategoryPadding} | {outputData}</format>
      
      <!-- When configured, spanName is prepended to outputData. -->
      <outputSpanNameOnLogTypes>
        <!-- available: Activated, Finished, SetTag, Log -->
        <Activated>true</Activated>
      </outputSpanNameOnLogTypes>

      <dataSerialization>
        <!-- available: Simple, Json. Simple is not machine-parseable -->
        <SetTag>Json</SetTag>
        <!-- available: SImple, Json, SimplifySingleKvpAndEventsOtherwiseJson -->
        <!-- SimplifySingleKvpAndEventsOtherwiseJson tries to remove json fluff in some scenarios, but reverts to json where needed -->
        <Log>Json</Log>
      </dataSerialization>
    </console>
```
