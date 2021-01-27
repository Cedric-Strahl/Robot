using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using SharpDX.XInput;

namespace XboxPolarGeneratorNs
{
    public class XboxPolarGenerator
    {
        private Controller _controller;
        private State _controllerState;
        private System.Threading.Timer _pollingTimer;

        private short _LthumbX; //max = +- 65536/2 = +- 32768
        private byte _Rtrig; //max = 0-255

        private float _LinearSpeed;
        private float _AngularSpeed;

        public XboxPolarGenerator(float MaxLinSpeed, float MaxAngSpeed, short LthumbXDeadZone, int pollInterval_ms)
        {
            _controller = new Controller(UserIndex.One);
           
            _pollingTimer = new Timer((c) =>
            {
                if(_controller.IsConnected)
                {
                    _controller.GetState(out _controllerState);
                    
                    _LthumbX = _controllerState.Gamepad.LeftThumbX;
                    _Rtrig = _controllerState.Gamepad.RightTrigger;


                    //deadZone LthumbX
                    if (_LthumbX > -LthumbXDeadZone && _LthumbX < LthumbXDeadZone) _LthumbX = 0;

                    if(_LthumbX > 0)
                        _LthumbX = map(_LthumbX, LthumbXDeadZone, (short)32767, 0, (short)32767);
                    if(_LthumbX < 0)
                        _LthumbX = map(_LthumbX, (short)-LthumbXDeadZone, (short)-32767, 0, (short)-32767);


                    _LinearSpeed = (MaxLinSpeed / 255) * _Rtrig;
                    _AngularSpeed = (MaxAngSpeed / 32768)*_LthumbX;


                    //calling event
                    OnPolarVectGenerated(_LinearSpeed, _AngularSpeed);


                }
            }, null, 0, pollInterval_ms);
        }

        short map(short x, short in_min, short in_max, short out_min, short out_max)
        {
            return (short)((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
        }

        public event EventHandler<OnPolarVectGeneratedEventArgs> OnPolarVectGeneratedEvent;

        public virtual void OnPolarVectGenerated(float linearSpeed, float angularSpeed)
        {
            var handler = OnPolarVectGeneratedEvent;
            if(handler != null)
            {
                handler(this, new OnPolarVectGeneratedEventArgs
                {
                    LinearSpeed = linearSpeed,
                    AngularSpeed = angularSpeed
                });
            }
        }
    }
}


public class OnPolarVectGeneratedEventArgs : EventArgs
{
    public float LinearSpeed;
    public float AngularSpeed;
}
