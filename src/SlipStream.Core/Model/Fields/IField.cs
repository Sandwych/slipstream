using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Model
{
    public interface IField : IFieldDescriptor
    {
        FieldValueGetter ValueGetter { get; }
        FieldDefaultValueGetter DefaultProc { get; }
        CriterionConverter CriterionConverter { get; }

        void VerifyDefinition();

        Dictionary<long, object> GetFieldValues(
            ICollection<Dictionary<string, object>> records);

        object SetFieldValue(object value);

        object BrowseField(IDictionary<string, object> record);

        #region Fluent interface

        IField WithLabel(string label);
        IField WithRequired();
        IField WithNotRequired();
        IField SetValueGetter(FieldValueGetter fieldGetter);
        IField WithDefaultValueGetter(FieldDefaultValueGetter defaultProc);
        IField SetCriterionConverter(CriterionConverter convProc);
        IField WithSize(int size);
        IField SetHelp(string help);
        IField Readonly();
        IField NotReadonly();
        IField OnDelete(OnDeleteAction act);
        IField BeProperty();
        IField SetOptions(IDictionary<string, string> options);
        IField WithUnique();
        IField NotUnique();

        #endregion

    }
}
