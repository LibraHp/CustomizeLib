#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""根据 CustomCore.AddPlantAlmanacStrings 第二个参数（图鉴名）完善 modpack.json 里各 DLL 的中文名。

规则：
  1. 读取 modpack.json 的 dllNames（DLL 文件名 -> 显示名）；
  2. 扫描 BepInEx 下所有 .csproj，用 <AssemblyName>（缺省为 csproj 文件名主干）建立
     “程序集名 -> 项目目录” 映射；
  3. 对每个 DLL：去掉 .dll 后缀得到程序集名，定位项目目录，在其 .cs 源码里找
     AddPlantAlmanacStrings(...) 调用，取第二个参数作为图鉴名；
  4. 名字按 CustomCore 的方式去掉 (id) / （…） 后缀（与注册时一致）；
  5. 仅当找到图鉴名时才更新；找不到则保持原值不变。
"""

from __future__ import annotations

import json
import os
import re
import sys
from pathlib import Path

NAME_STRIP_RE = re.compile(r"[\(（].*[\)）]")
CALL_RE = re.compile(r"AddPlantAlmanacStrings\s*\(")
STRING_LITERAL_RE = re.compile(r'"((?:[^"\\]|\\.)*)"')
APPEND_LITERAL_RE = re.compile(r'AppendLiteral\(\s*"((?:[^"\\]|\\.)*)"\s*\)')
ASSEMBLY_NAME_RE = re.compile(r"<AssemblyName>\s*([^<]+?)\s*</AssemblyName>")

IGNORED_DIRS = {".git", "bin", "obj", ".vs"}


def iter_cs_files(root: Path):
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if d not in IGNORED_DIRS]
        for fn in filenames:
            if fn.endswith(".cs"):
                yield Path(dirpath) / fn


def split_call_args(text: str, open_idx: int) -> list[str]:
    """text[open_idx] == '('，按顶层逗号切分该调用的实参，返回各实参原文。"""
    depth = 0
    args: list[str] = []
    cur = open_idx + 1
    i = open_idx
    n = len(text)
    in_str = False
    esc = False
    while i < n:
        c = text[i]
        if in_str:
            if esc:
                esc = False
            elif c == "\\":
                esc = True
            elif c == '"':
                in_str = False
            i += 1
            continue
        if c == '"':
            in_str = True
            i += 1
            continue
        if c == "(":
            depth += 1
        elif c == ")":
            depth -= 1
            if depth == 0:
                arg = text[cur:i].strip()
                if arg:
                    args.append(arg)
                return args
        elif c == "," and depth == 1:
            args.append(text[cur:i].strip())
            cur = i + 1
        i += 1
    return args


def name_from_arg(arg: str, file_text: str) -> str | None:
    if "ToStringAndClear" in arg:
        parts = APPEND_LITERAL_RE.findall(file_text)
        if not parts:
            return None
        raw = "".join(parts)
    else:
        parts = STRING_LITERAL_RE.findall(arg)
        if not parts:
            return None
        raw = "".join(parts)
    name = NAME_STRIP_RE.sub("", raw).strip()
    return name or None


def almanac_names_for_dir(cs_dir: Path) -> list[str]:
    names: list[str] = []
    for path in sorted(iter_cs_files(cs_dir)):
        try:
            text = path.read_text(encoding="utf-8-sig", errors="surrogateescape")
        except OSError:
            continue
        for m in CALL_RE.finditer(text):
            args = split_call_args(text, m.end() - 1)
            if len(args) < 2:
                continue
            name = name_from_arg(args[1], text)
            if name:
                names.append(name)
    return names


def build_assembly_map(root: Path) -> dict[str, Path]:
    mapping: dict[str, Path] = {}
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if d not in IGNORED_DIRS]
        for fn in filenames:
            if not fn.endswith(".csproj"):
                continue
            csproj = Path(dirpath) / fn
            try:
                text = csproj.read_text(encoding="utf-8-sig", errors="surrogateescape")
            except OSError:
                continue
            m = ASSEMBLY_NAME_RE.search(text)
            asm = m.group(1).strip() if m else csproj.stem
            mapping.setdefault(asm, csproj.parent)
    return mapping


def main() -> int:
    root = Path(__file__).resolve().parent
    modpack_path = root / "modpack.json"
    bepinex = root / "BepInEx"

    raw = modpack_path.read_bytes()
    bom = raw.startswith(b"\xef\xbb\xbf")
    body = raw[3:] if bom else raw
    data = json.loads(body.decode("utf-8"))

    dll_names = data.get("dllNames", {})
    if not isinstance(dll_names, dict):
        print("error: dllNames 不是对象", file=sys.stderr)
        return 2

    asm_map = build_assembly_map(bepinex)
    print(f"扫描到 {len(asm_map)} 个程序集映射。\n")

    updated = 0
    multi: list[tuple[str, list[str]]] = []
    for dll in list(dll_names.keys()):
        asm = dll[:-4] if dll.endswith(".dll") else dll
        cs_dir = asm_map.get(asm)
        old = dll_names[dll]
        if cs_dir is None:
            print(f"跳过  {dll}: 未找到项目目录")
            continue
        names = almanac_names_for_dir(cs_dir)
        if not names:
            print(f"保持  {dll}: 未找到 AddPlantAlmanacStrings（原值 {old!r}）")
            continue
        uniq = list(dict.fromkeys(names))
        new = uniq[0]
        if len(uniq) > 1:
            multi.append((dll, uniq))
        if new != old:
            dll_names[dll] = new
            updated += 1
            print(f"更新  {dll}: {old!r} -> {new!r}")
        else:
            print(f"不变  {dll}: {new!r}")

    if multi:
        print("\n注意：以下 DLL 注册了多个图鉴名，取第一个：")
        for dll, names in multi:
            print(f"  {dll}: {names}")

    out = json.dumps(data, ensure_ascii=False, indent=2)
    if body.endswith(b"\n"):
        out += "\n"
    out_bytes = out.encode("utf-8")
    if bom:
        out_bytes = b"\xef\xbb\xbf" + out_bytes
    modpack_path.write_bytes(out_bytes)

    print(f"\n已更新 {updated} 个 DLL 名称，写入 {modpack_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
