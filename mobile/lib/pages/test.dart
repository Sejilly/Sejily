import 'package:flutter/material.dart';

class SearchableMultiSelectDropdown extends StatefulWidget {
  final String label;
  final String hintText;
  final List<String>? suggestions;
  final List<String>? selectedItems;
  final Function(List<String>) onChanged;
  final bool optional;

  const SearchableMultiSelectDropdown({
    super.key,
    required this.label,
    required this.hintText,
    required this.suggestions,
    required this.selectedItems,
    required this.onChanged,
    this.optional = true,
  });

  @override
  State<SearchableMultiSelectDropdown> createState() =>
      _SearchableMultiSelectDropdownState();
}

class _SearchableMultiSelectDropdownState
    extends State<SearchableMultiSelectDropdown> {
  final MenuController _menu = MenuController();
  final TextEditingController _searchController = TextEditingController();
  List<String> filtereditem = [];

  @override
  void initState() {
    super.initState();
    filtereditem = widget.suggestions ?? [];
  }

  void _onSearchChanged(String query) {
    setState(() {
      if (query.trim().isEmpty) {
        filtereditem = widget.suggestions ?? [];
      } else {
        filtereditem = (widget.suggestions ?? [])
            .where((item) => item.toLowerCase().contains(query.toLowerCase()))
            .toList();
      }
    });

    if (!_menu.isOpen) {
      _menu.open();
    }
  }

  void _toggleItem(String item) {
    final List<String> update = List.from(widget.selectedItems ?? []);
    if (update.contains(item)) {
      update.remove(item);
    } else {
      update.add(item);
    }
    widget.onChanged(update);
  }

  void _addCustomText() {
    String customText = _searchController.text.trim();
    if (customText.isNotEmpty &&
        !(widget.selectedItems ?? []).contains(customText)) {
      final List<String> update =
          List.from(widget.selectedItems ?? [])..add(customText);
      widget.onChanged(update);
      _searchController.clear();
      _onSearchChanged('');
    }
  }

  @override
  Widget build(BuildContext context) {
    final selected = widget.selectedItems ?? [];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text.rich(
          TextSpan(
            text: widget.label,
            style: const TextStyle(color: Color(0xff0B192C), fontSize: 14),
            children: widget.optional
                ? [
                    const TextSpan(
                      text: ' (إن وُجدت)',
                      style: TextStyle(color: Color(0XFF94A3B8)),
                    )
                  ]
                : [],
          ),
        ),
        const SizedBox(height: 4),

        LayoutBuilder(
          builder: (context, constrained) {
            return MenuAnchor(
              controller: _menu,
              crossAxisUnconstrained: false,
              style: MenuStyle(
                minimumSize: WidgetStatePropertyAll(
                  Size(constrained.maxWidth, 0),
                ),
                maximumSize: WidgetStatePropertyAll(
                  Size(constrained.maxWidth, 200),
                ),
                alignment: AlignmentDirectional.bottomStart,
                backgroundColor:
                    const WidgetStatePropertyAll(Color(0xffffffff)),
                surfaceTintColor:
                    const WidgetStatePropertyAll(Colors.transparent),
                side: const WidgetStatePropertyAll(
                  BorderSide(color: Color(0xff2B6CB0), width: 1),
                ),
                shape: WidgetStatePropertyAll(
                  RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                elevation: const WidgetStatePropertyAll(4),
              ),
              menuChildren: [
                ...filtereditem.map((item) {
                  final isSelected = selected.contains(item);
                  return MenuItemButton(
                    closeOnActivate: false,
                    onPressed: () => _toggleItem(item),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          item,
                          style: TextStyle(
                            color: isSelected
                                ? const Color(0xff2B6CB0)
                                : const Color(0xff475569),
                            fontWeight: isSelected
                                ? FontWeight.w600
                                : FontWeight.w400,
                          ),
                        ),
                        Icon(
                          isSelected
                              ? Icons.check_box_rounded
                              : Icons.check_box_outline_blank_rounded,
                          color: isSelected
                              ? const Color(0xff2B6CB0)
                              : const Color(0xffCBD5E1),
                          size: 20,
                        ),
                      ],
                    ),
                  );
                }),

                if (_searchController.text.trim().isNotEmpty &&
                    !filtereditem.contains(_searchController.text.trim()) &&
                    !selected.contains(_searchController.text.trim()))
                  MenuItemButton(
                    onPressed: _addCustomText,
                    child: Row(
                      children: [
                        const Icon(Icons.add_circle_outline,
                            color: Color(0xff2B6CB0), size: 18),
                        const SizedBox(width: 8),
                        Text(
                          'إضافة "${_searchController.text}"',
                          style: const TextStyle(
                            color: Color(0xff2B6CB0),
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                  ),
              ],
              builder: (context, controller, child) {
                return InputDecorator(
                  decoration: InputDecoration(
                    contentPadding: const EdgeInsets.symmetric(
                      horizontal: 12,
                      vertical: 6,
                    ),
                    filled: true,
                    fillColor: Colors.white,
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                      borderSide: const BorderSide(color: Color(0xffCBD5E1)),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                      borderSide: const BorderSide(color: Color(0xff2B6CB0)),
                    ),
                  ),
                  child: Row(
                    children: [
                      // عرض العناصر المحددة كـ Chips داخل الـ Field بجانب بعضها
                      if (selected.isNotEmpty)
                        Expanded(
                          child: SingleChildScrollView(
                            scrollDirection: Axis.horizontal,
                            child: Row(
                              children: selected.map((item) {
                                return Padding(
                                  padding: const EdgeInsets.only(left: 4),
                                  child: Chip(
                                    materialTapTargetSize:
                                        MaterialTapTargetSize.shrinkWrap,
                                    visualDensity: VisualDensity.compact,
                                    label: Text(
                                      item,
                                      style: const TextStyle(
                                        fontSize: 11,
                                        color: Color(0xff2B6CB0),
                                      ),
                                    ),
                                    backgroundColor: const Color(0xffEBF8FF),
                                    deleteIcon: const Icon(
                                      Icons.close,
                                      size: 14,
                                      color: Color(0xff2B6CB0),
                                    ),
                                    onDeleted: () => _toggleItem(item),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(12),
                                      side: const BorderSide(
                                        color: Color(0xff2B6CB0),
                                        width: 0.5,
                                      ),
                                    ),
                                  ),
                                );
                              }).toList(),
                            ),
                          ),
                        ),

                      // حقل الكتابة للبحث بجانب الـ Chips
                      Expanded(
                        flex: selected.isEmpty ? 1 : 0,
                        child: ConstrainedBox(
                          constraints: const BoxConstraints(minWidth: 80),
                          child: TextField(
                            controller: _searchController,
                            onChanged: _onSearchChanged,
                            onTap: () {
                              if (!controller.isOpen) controller.open();
                            },
                            style: const TextStyle(
                              fontSize: 13,
                              color: Color(0xff0B192C),
                            ),
                            decoration: InputDecoration(
                              hintText:
                                  selected.isEmpty ? widget.hintText : "",
                              hintStyle: const TextStyle(
                                fontSize: 13,
                                color: Color(0xff94A3B8),
                              ),
                              border: InputBorder.none,
                              isDense: true,
                              contentPadding:
                                  const EdgeInsets.symmetric(vertical: 8),
                            ),
                          ),
                        ),
                      ),

                      const Icon(Icons.search,
                          color: Color(0xff94A3B8), size: 20),
                    ],
                  ),
                );
              },
            );
          },
        ),
      ],
    );
  }
}