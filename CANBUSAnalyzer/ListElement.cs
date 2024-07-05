﻿using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TeslaSCAN
{
  public class ListElement : INotifyPropertyChanged
  {
    public uint packetId { get; set; }
    public string idHex
    {
      get
      {
        return System.Convert.ToString(packetId, 16).ToUpper().PadLeft(3, '0');
      }
    }
    public string name { get; set; }
    private object value;
    public object Current
    {
      get
      {
        return value;
      }
    }

    public ConcurrentStack<DataPoint> Points { get; set; }
    public LineSeries Line { get; private set; }

    public string unit { get; set; }
    public int index;
    public double max { get; set; }
    public double min { get; set; }
    public bool changed;
    public bool selected;
    public string tag;
    public int viewType;
    public long timeStamp;
    public List<int> bits = new List<int>();
    public int numBits
    {
      get
      {
        return bits.Any() ? bits.Last() - bits.First() + 1 : 0;
      }
    }
    public double scaling { get; set; }
    public object previous;

    public override string ToString()
    {
      return value.ToString();
    }

    public double Convert(double val, bool convertToImperial)
    {
      if (!convertToImperial)
      {
        return val;
      }

      if ((unit.ToUpper() == "C") || (unit == "zCC"))
      {
        return val * 1.8 + 32;
      }

      if (unit == "Nm")
      {
        return val * Parser.nm_to_ftlb;
      }

      if (unit == "wh|km")
      {
        return val * Parser.miles_to_km;
      }

      if (unit.ToLower().Contains("km"))
      {
        return val / Parser.miles_to_km;
      }

      return val;
    }

    public string GetUnit(bool convertToImperial)
    {
      if (!convertToImperial)
      {
        return unit;
      }

      if ((unit.ToUpper() == "C") || (unit == "zCC"))
      {
        return "F";
      }

      if (unit == "Nm")
      {
        return "LbFt";
      }

      if (unit == "wh|km")
      {
        return "wh|mi";
      }

      if (unit.ToLower().Contains("km"))
      {
        return unit.ToLower().Replace("km", "mi");
      }

      return unit;
    }

    public object GetValue(bool convertToImperial)
    {
      if (convertToImperial && value is double)
      {
        return Convert((double)value, convertToImperial);
      }
      else
      {
        return value;
      }
    }

    public void SetValue(object val)
    {
      previous = value;
      changed = !value.Equals(val);
      value = val;

      if (changed) {
        NotifyPropertyChanged("Current");
      }

      if (!(val is string)) {

        double valueD = System.Convert.ToDouble(val);

        if (valueD > max) {
          max = valueD;
        }

        if (valueD < min) {
          min = valueD;
        }


#if VERBOSE
      Console.WriteLine(this.name + " " + val);
#endif
        Points.Push(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(DateTime.Now), valueD));
        NotifyPropertyChanged("Points");
      }
      /*if (Points.Count > 1)
      {
        double dt = Points[Points.Count - 1].X - Points[Points.Count - 2].X;
        double a = dt / (0.99 + dt);
        Points[Points.Count - 1] = new DataPoint(Points[Points.Count - 1].X, Points[Points.Count - 2].Y * (1-a) + Points[Points.Count - 1].Y * a);
      }*/
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(String propertyName = "")
    {
     if (PropertyChanged != null)
     {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
     }
    }

    public ListElement(string name, string unit, string tag, int index, object value, uint packetId)
    {
      this.name = name;
      this.unit = unit;
      this.tag = tag;
      this.index = index;
      this.value = value;
      if (value is double)
        this.max = this.min = (double)value;
      this.packetId = packetId;
      changed = true;

      this.Points = new ConcurrentStack<DataPoint>();
    }
  }
}
