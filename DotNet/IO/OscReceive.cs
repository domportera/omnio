using System.ComponentModel;
using System.Runtime.InteropServices;
using OperatorCore;
using Rug.Osc;
using Utilities.Logging;

namespace IO;

[Category("IO"), Description("Receives OSC messages"), Guid("76944796-9395-44CB-81EC-F6716C77DE57")]
public sealed class OscReceive : GraphNodeLogic
{
    private readonly InputSlot<int> _port = new(0, 8000);
    private readonly InputSlot<string> _oscAddress = new(1, "/test");
    private readonly OutputSlot<Rug.Osc.OscMessage[]> _output = new(0);

    private OscReceive()
    {
        _messageReceivedMethod = OnMessageReceived;
        _unknownAddressMethod = OnUnknownAddress;
    }

    protected override void OnInitialize()
    {
        _port.ValueChanged += OnPortChanged;
        _oscAddress.ValueChanged += OnAddressChanged;
    }

    private void OnAddressChanged()
    {
        if (_receiver == null)
        {
            return;
        }

        if (_previousAddress != null)
        {
            _receiver.Detach(_previousAddress, _messageReceivedMethod);
        }

        UpdateAddress(_receiver);
    }

    private void OnMessageReceived(OscMessage message)
    {
        LogLady.Debug($"Received message: {message}");
        if (_output.Value is { Length: 1 })
        {
            _output.Value[0] = message;
            _output.UpdateAsReferenceType();
        }
        else
        {
            _output.Value = [message];
        }
    }

    private void OnPortChanged()
    {
        DisposeReceiver();

        _previousAddress = null;
        _receiver = new OscListener(_port.Value);

        try
        {
            _receiver.Connect();
            _receiver.UnknownAddress += _unknownAddressMethod;
            UpdateAddress(_receiver);
        }
        catch (Exception e)
        {
            _receiver.Close();
            _receiver.Dispose();
            _receiver = null;
            LogLady.Error(e.Message);
        }
    }

    private void DisposeReceiver()
    {
        if (_receiver == null) return;
        
        _receiver.UnknownAddress -= _unknownAddressMethod;
        _receiver.Detach(_previousAddress, _messageReceivedMethod);
        _receiver.Close();
        _receiver.Dispose();
    }

    private void UpdateAddress(OscListener receiver)
    {
        var oscAddress = _oscAddress.Value;
        receiver.Attach(oscAddress, _messageReceivedMethod);
        _previousAddress = oscAddress;
    }

    private void OnUnknownAddress(object? sender, UnknownAddressEventArgs e)
    {
    }

    public override void Process(double deltaTime)
    {
    }

    protected override void OnDestroy()
    {
        DisposeReceiver();
    }
    
    private OscListener? _receiver;
    private string? _previousAddress = null;
    private readonly OscMessageEvent _messageReceivedMethod;
    private readonly EventHandler<UnknownAddressEventArgs> _unknownAddressMethod;
}