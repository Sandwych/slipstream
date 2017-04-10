using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    public interface IFieldCollection : IDictionary<string, IField>
    {
        IField Integer(string name);
        IField BigInteger(string name);
        IField Double(string name);
        IField Decimal(string name);
        IField Boolean(string name);
        IField DateTime(string name);
        IField Date(string name);
        IField Time(string name);
        IField Chars(string name);
        IField Text(string name);
        IField Xml(string name);
        IField Binary(string name);


        IField ManyToOne(string name, string masterEntity);
        IField OneToMany(string name, string childEntity, string relatedField);
        IField ManyToMany(string name, string refEntity, string originField, string targetField);

        IField Enumeration(string name, IDictionary<string, string> options);
        IField Reference(string name);
    }
}
