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

### Programmatic configuration

Usually in a deployed app you'll want to use app.config, but while testing (which is what this library is targetted at anyway! Silly author, excluding that in the v0!) you'll want to avoid making an app.config if you don't have one already. In that case use code configuration like so:

\[caveat: This is code within a document file, and likely to get out of date. Please use this example + intellisense suggestions in your IDE as guidance rather than expecting it to be copy-paste-runable. For copy-paste-run, see the \*.cs files in the project\]
```C#
var builder = TracingConfigurationBuilder.Instance
    // no file tracing
    //.WithFileTracing
    .WithConsoleTracing(
        "[{date:h:mm:ss tt}] {spanId}{spanIdFloatPadding} | {logCategory}{logCategoryPadding} | {outputData}",
        settings => settings
            .WithColorMode(ColorMode.BasedOnCategory)
            .WithColorsForTheBasedOnCategoryColorMode(
                ConsoleColor.Green,
                ConsoleColor.Red,
                ConsoleColor.Magenta,
                ConsoleColor.Blue)
            .WithOutputSpanNameOnCategory(
                activated: true,
                finished: true)
            .WithOutputDurationOnFinished(true)
            .WithDataSerialization(
                SetTagDataSerialization.Simple,
                LogDataSerialization.Simple));
consoleConfiguration = builder.BuildConsoleConfiguration();
fileElement = builder.BuildFileConfiguration();

```

### System.Configuration (app.config)

The configuration type is configured like so:
```xml
  <configSections>
    <section name="tracing" type="OpenTracing.Contrib.LocalTracers.Config.System_Configuration.TracingConfigurationSection, OpenTracing.Contrib.LocalTracers"/>
  </configSections>
```

You can then read it like so:
```C#
((TracingConfigurationSection) ConfigurationManager.GetSection("tracing"));
```

#### File Configuration
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

#### Console Configuration
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

      <!-- [4:21:01 pm] | 1.0.2.1   | StartSpan  | Whatever text is configured in the method -->
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
        <Finished>true</Finished>
      </outputSpanNameOnLogTypes>

      <outputDurationOnFinished>true</outputDurationOnFinished>

      <dataSerialization>
        <!-- available: Simple, Json. Simple is not machine-parseable -->
        <SetTag>Simple</SetTag>
        <!-- available: SImple, Json, SimplifySingleKvpAndEventsOtherwiseJson -->
        <!-- SimplifySingleKvpAndEventsOtherwiseJson tries to remove json fluff in some scenarios, but reverts to json where needed -->
        <Log>Simple</Log>
      </dataSerialization>
    </console>
```
