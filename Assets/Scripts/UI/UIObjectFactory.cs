﻿using System;
using System.Collections.Generic;
#if FAIRYGUI_TOLUA
using LuaInterface;
#endif

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class UIObjectFactory
    {
        public delegate GComponent GComponentCreator();
        public delegate GList GListCreator();
        public delegate GLoader GLoaderCreator();
        public delegate GLoader3D GLoader3DCreator();
        public delegate GTextField GTextFieldCreator();
        public delegate GRichTextField GRichTextFieldCreator();
        public delegate GTextInput GTextInputCreator();

        static Dictionary<string, GComponentCreator> packageItemExtensions = new Dictionary<string, GComponentCreator>();
        static GRichTextFieldCreator richTextFieldCreator;
        static GTextFieldCreator textFieldCreator;
        static GTextInputCreator textInputCreator;
        static GLoaderCreator loaderCreator;
        static GLoader3DCreator loader3DCreator;
        static GListCreator listCreator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        public static void SetPackageItemExtension(string url, System.Type type)
        {
            SetPackageItemExtension(url, () => { return (GComponent)Activator.CreateInstance(type); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="creator"></param>
        public static void SetPackageItemExtension(string url, GComponentCreator creator)
        {
            if (url == null)
                throw new Exception("Invaild url: " + url);

            PackageItem pi = UIPackage.GetItemByURL(url);
            if (pi != null)
                pi.extensionCreator = creator;

            packageItemExtensions[url] = creator;
        }

#if FAIRYGUI_TOLUA
        public static void SetExtension(string url, System.Type baseType, LuaFunction extendFunction)
        {
            SetPackageItemExtension(url, () =>
            {
                GComponent gcom = (GComponent)Activator.CreateInstance(baseType);

                extendFunction.BeginPCall();
                extendFunction.Push(gcom);
                extendFunction.PCall();
                gcom.SetLuaPeer(extendFunction.CheckLuaTable());
                extendFunction.EndPCall();

                return gcom;
            });
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public static void SetLoaderExtension(System.Type type)
        {
            loaderCreator = () => { return (GLoader)Activator.CreateInstance(type); };
        }
        
        public static void SetListExtension(System.Type type)
        {
            listCreator = () => { return (GList)Activator.CreateInstance(type); };
        }
        
        public static void SetTextFieldExtension(System.Type type)
        {
            textFieldCreator = () => { return (GTextField)Activator.CreateInstance(type); };
        }
        
        public static void SetTextInputExtension(System.Type type)
        {
            textInputCreator = () => { return (GTextInput)Activator.CreateInstance(type); };
        }
        
        public static void SetRichTextFieldExtension(System.Type type)
        {
            richTextFieldCreator = () => { return (GRichTextField)Activator.CreateInstance(type); };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creator"></param>
        public static void SetLoaderExtension(GLoaderCreator creator)
        {
            loaderCreator = creator;
        }
        
        public static void SetListExtension(GListCreator creator)
        {
            listCreator = creator;
        }
        
        public static void SetTextFieldExtension(GTextFieldCreator creator)
        {
            textFieldCreator = creator;
        }
        
        public static void SetTextInputExtension(GTextInputCreator creator)
        {
            textInputCreator = creator;
        }
        
        public static void SetRichTextFieldExtension(GRichTextFieldCreator creator)
        {
            richTextFieldCreator = creator;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public static void SetLoader3DExtension(System.Type type)
        {
            loader3DCreator = () => { return (GLoader3D)Activator.CreateInstance(type); };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creator"></param>
        public static void SetLoader3DExtension(GLoader3DCreator creator)
        {
            loader3DCreator = creator;
        }

        internal static void ResolvePackageItemExtension(PackageItem pi)
        {
            if (!packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.id + pi.id, out pi.extensionCreator)
                && !packageItemExtensions.TryGetValue(UIPackage.URL_PREFIX + pi.owner.name + "/" + pi.name, out pi.extensionCreator))
                pi.extensionCreator = null;
        }

        public static void ClearExtension(string url)
        {
            if (packageItemExtensions.ContainsKey(url))
                packageItemExtensions.Remove(url);
        }

        public static void Clear()
        {
            packageItemExtensions.Clear();
            loaderCreator = null;
            listCreator = null;
            loader3DCreator = null;
            textFieldCreator = null;
            textInputCreator = null;
            richTextFieldCreator = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="userClass"></param>
        /// <returns></returns>
        public static GObject NewObject(PackageItem pi, System.Type userClass = null)
        {
            GObject obj;
            if (pi.type == PackageItemType.Component)
            {
                if (userClass != null)
                {
                    Stats.LatestObjectCreation++;
                    obj = (GComponent)Activator.CreateInstance(userClass);
                }
                else if (pi.extensionCreator != null)
                {
                    Stats.LatestObjectCreation++;
                    obj = pi.extensionCreator();
                }
                else
                    obj = NewObject(pi.objectType);
            }
            else
                obj = NewObject(pi.objectType);

            if (obj != null)
                obj.packageItem = pi;

            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GObject NewObject(ObjectType type)
        {
            Stats.LatestObjectCreation++;

            switch (type)
            {
                case ObjectType.Image:
                    return new GImage();

                case ObjectType.MovieClip:
                    return new GMovieClip();

                case ObjectType.Component:
                    return new GComponent();

                case ObjectType.Text:
                    if (textFieldCreator != null)
                        return textFieldCreator();
                    return new GTextField();

                case ObjectType.RichText:
                    if (richTextFieldCreator != null)
                        return richTextFieldCreator();
                    return new GRichTextField();

                case ObjectType.InputText:
                    if (textInputCreator != null)
                        return textInputCreator();
                    return new GTextInput();

                case ObjectType.Group:
                    return new GGroup();

                case ObjectType.List:
                    if (listCreator != null)
                        return listCreator();
                    return new GList();

                case ObjectType.Graph:
                    return new GGraph();

                case ObjectType.Loader:
                    if (loaderCreator != null)
                        return loaderCreator();
                    else
                        return new GLoader();

                case ObjectType.Button:
                    return new GButton();

                case ObjectType.Label:
                    return new GLabel();

                case ObjectType.ProgressBar:
                    return new GProgressBar();

                case ObjectType.Slider:
                    return new GSlider();

                case ObjectType.ScrollBar:
                    return new GScrollBar();

                case ObjectType.ComboBox:
                    return new GComboBox();

                case ObjectType.Tree:
                    return new GTree();

                case ObjectType.Loader3D:
                    if (loader3DCreator != null)
                        return loader3DCreator();
                    else
                        return new GLoader3D();
                default:
                    return null;
            }
        }
    }
}
