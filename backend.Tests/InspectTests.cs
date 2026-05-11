using System;
using AutoMapper;
using Xunit;
using Xunit.Abstractions;

namespace backend.Tests;

public class InspectTests {
    private readonly ITestOutputHelper _output;
    public InspectTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void InspectAutoMapper() {
        var type = typeof(MapperConfiguration);
        _output.WriteLine($"Type: {type.FullName}");
        foreach (var ctor in type.GetConstructors()) {
            _output.WriteLine($"Ctor: {ctor}");
        }
    }
}
