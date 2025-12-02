class SoilType {
  final int id;
  final String typeName;

  SoilType({
    required this.id,
    required this.typeName,
  });

  factory SoilType.fromJson(Map<String, dynamic> json) {
    return SoilType(
      id: json['id'] as int,
      typeName: json['typeName'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'typeName': typeName,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SoilType &&
          runtimeType == other.runtimeType &&
          id == other.id &&
          typeName == other.typeName;

  @override
  int get hashCode => id.hashCode ^ typeName.hashCode;
}

