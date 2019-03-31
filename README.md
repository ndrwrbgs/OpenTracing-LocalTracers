# OpenTracing-LocalTracers
Wrap your existing tracer with the ability to log to a local file and/or the Console.

## Commonly used types
* `TracingConfigurationSection`
* `ColoredConsoleTracerDecorationFactory`
* `FileTracerDecorationFactory`

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
