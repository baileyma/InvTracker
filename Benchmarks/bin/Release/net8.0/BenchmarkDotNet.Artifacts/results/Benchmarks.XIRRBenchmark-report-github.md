```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i5-1235U 2.50GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2


```
| Method          | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD |
|---------------- |---------:|---------:|---------:|---------:|------:|--------:|
| XIRRCalculation | 31.78 μs | 2.218 μs | 6.541 μs | 30.05 μs |  1.04 |    0.30 |
