using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IField : IFieldDescriptor
    {
        FieldValueGetter ValueGetter { get; }
        FieldDefaultValueGetter DefaultProc { get; }
        CriterionConverter CriterionConverter { get; }

        void VerifyDefinition();

        Dictionary<long, object> GetFieldValues(
            IServiceContext scope, ICollection<Dictionary<string, object>> records);

        object SetFieldValue(IServiceContext scope, object value);

        object BrowseField(IDictionary<string, object> record);

        #region Fluent interface

        IField SetLabel(string label);
        IField Required();
        IField NotRequired();
        IField SetValueGetter(FieldValueGetter fieldGetter);
        IField SetDefaultValueGetter(FieldDefaultValueGetter defaultProc);
        IField SetCriterionConverter(CriterionConverter convProc);
        IField SetSize(int size);
        IField SetHelp(string help);
        IField Readonly();
        IField NotReadonly();
        IField OnDelete(OnDeleteAction act);
        IField BeProperty();
        IField SetOptions(IDictionary<string, string> options);
        IField Unique();
        IField NotUnique();

        #endregion

    }
}
