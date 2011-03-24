using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IFieldCollection : IDictionary<string, IField>
    {
        IField Integer(string name);
        IField BigInteger(string name);
        IField Float(string name);
        IField Money(string name);
        IField Boolean(string name);
        IField DateTime(string name);
        IField Chars(string name);
        IField Text(string name);
        IField Binary(string name);


        IField ManyToOne(string name, string masterModel);
        IField OneToMany(string name, string childModel, string relatedField);
        IField ManyToMany(string name, string refModel, string originField, string targetField);

        IField Enumeration(string name, IDictionary<string, string> options);
        IField Reference(string name);

        IField Version();
    }
}
