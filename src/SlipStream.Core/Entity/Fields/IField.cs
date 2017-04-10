using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
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
        IField WithValueGetter(FieldValueGetter fieldGetter);
        IField WithDefaultValueGetter(FieldDefaultValueGetter defaultProc);
        IField WithCriterionConverter(CriterionConverter convProc);
        IField WithSize(int size);
        IField WithHelp(string help);
        IField WithReadonly();
        IField WithNotReadonly();
        IField OnDelete(OnDeleteAction act);
        IField BeProperty();
        IField WithOptions(IDictionary<string, string> options);
        IField WithUnique();
        IField WithNotUnique();

        #endregion

    }
}
